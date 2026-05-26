using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orchestrator.Runner.Agent;
using Orchestrator.Runner.Configuration;

namespace Orchestrator.Runner.Container;

public sealed class ContainerCleanupService : BackgroundService
{
    private readonly PodmanCli _podman;
    private readonly RunnerState _state;
    private readonly RunnerOptions _options;
    private readonly ILogger<ContainerCleanupService> _logger;

    public ContainerCleanupService(
        PodmanCli podman,
        RunnerState state,
        RunnerOptions options,
        ILogger<ContainerCleanupService> logger
    )
    {
        _podman = podman;
        _state = state;
        _options = options;
        _logger = logger;
    }

    // Periodic cleanup loop: runs every CleanupInterval, lists orphaned containers
    // (those with orchestrator.job_id label that don't match any active job),
    // stops and removes them, then cleans workspace directories.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanOrphansAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Container cleanup cycle failed");
            }

            await Task.Delay(_options.CleanupInterval, stoppingToken);
        }
    }

    // Best-effort stop (with graceful period) followed by force-remove.
    public async Task StopAndRemoveAsync(string containerId, CancellationToken ct)
    {
        try
        {
            await _podman.StopAsync(containerId, _options.ContainerGracefulStop, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to stop container {ContainerId}", containerId);
        }

        try
        {
            await _podman.RemoveAsync(containerId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove container {ContainerId}", containerId);
        }
    }

    private async Task CleanOrphansAsync(CancellationToken ct)
    {
        _logger.LogInformation("Running container cleanup cycle");

        var activeIds = _state.ActiveJobIds;
        var orphans = await _podman.ListOrphanedAsync(activeIds, ct);

        foreach (var containerId in orphans)
        {
            _logger.LogInformation("Cleaning orphaned container {ContainerId}", containerId);
            await StopAndRemoveAsync(containerId, ct);
        }

        if (Directory.Exists(_options.WorkspacePath))
        {
            foreach (var dir in Directory.GetDirectories(_options.WorkspacePath))
            {
                try
                {
                    Directory.Delete(dir, recursive: true);
                    _logger.LogInformation("Cleaned workspace {Dir}", dir);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to clean workspace {Dir}", dir);
                }
            }
        }
    }

    // Single-shot cleanup used at startup (RunnerAgent.StartAsync) before accepting jobs.
    // Returns the count of removed containers for logging.
    public async Task<int> CleanAsync(string runnerId, CancellationToken ct)
    {
        var activeIds = _state.ActiveJobIds;
        var orphans = await _podman.ListOrphanedAsync(activeIds, ct);
        var count = 0;

        foreach (var containerId in orphans)
        {
            await StopAndRemoveAsync(containerId, ct);
            count++;
        }

        return count;
    }
}
