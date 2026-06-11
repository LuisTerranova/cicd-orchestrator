using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Domain.ValueObjects;

namespace Orchestrator.Infrastructure.Services;

public sealed class RunnerExpiryOptions
{
    public TimeSpan InactivityThreshold { get; set; } = TimeSpan.FromMinutes(2);
}

public sealed class RunnerExpiryService : BackgroundService
{
    private readonly IRunnerRepository _runnerRepository;
    private readonly IOptions<RunnerExpiryOptions> _options;
    private readonly ILogger<RunnerExpiryService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(30);

    public RunnerExpiryService(
        IRunnerRepository runnerRepository,
        IOptions<RunnerExpiryOptions> options,
        ILogger<RunnerExpiryService> logger
    )
    {
        _runnerRepository = runnerRepository;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RunnerExpiryService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExpireStaleRunnersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Runner expiry cycle failed");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task ExpireStaleRunnersAsync(CancellationToken ct)
    {
        var threshold = DateTime.UtcNow - _options.Value.InactivityThreshold;

        var idleRunners = await _runnerRepository.GetByStatusAsync(RunnerStatus.Idle, ct);
        var busyRunners = await _runnerRepository.GetByStatusAsync(RunnerStatus.Busy, ct);

        var staleRunners = idleRunners
            .Concat(busyRunners)
            .Where(r => r.LastSeen < threshold)
            .ToList();

        foreach (var runner in staleRunners)
        {
            runner.Disconnect();
            await _runnerRepository.UpdateAsync(runner, ct);
            _logger.LogWarning(
                "Runner {Id} ({Name}) expired - last seen {LastSeen:O}",
                runner.Id, runner.Name, runner.LastSeen
            );
        }

        if (staleRunners.Count > 0)
            _logger.LogInformation("Expired {Count} stale runner(s)", staleRunners.Count);
    }
}
