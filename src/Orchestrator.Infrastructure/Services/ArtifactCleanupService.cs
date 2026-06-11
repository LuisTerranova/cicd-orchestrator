using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Infrastructure.Configuration;

namespace Orchestrator.Infrastructure.Services;

public sealed class ArtifactCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<StorageOptions> _storage;
    private readonly ILogger<ArtifactCleanupService> _logger;

    public ArtifactCleanupService(
        IServiceScopeFactory scopeFactory,
        IOptions<StorageOptions> storage,
        ILogger<ArtifactCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _storage = storage;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ArtifactCleanupService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRun = now.Date.AddDays(1).AddHours(3);
            var delay = nextRun - now;

            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, stoppingToken);

            if (stoppingToken.IsCancellationRequested)
                break;

            try
            {
                await RunCleanupCycleAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Artifact cleanup cycle failed");
            }
        }
    }

    private async Task RunCleanupCycleAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var artifactDir = _storage.Value.ArtifactsPath;

        if (!Directory.Exists(artifactDir))
            return;

        var now = DateTime.UtcNow;
        var dirs = Directory.GetDirectories(artifactDir);

        foreach (var dir in dirs)
        {
            try
            {
                var dirInfo = new DirectoryInfo(dir);
                var age = now - dirInfo.CreationTimeUtc;
                if (age > TimeSpan.FromDays(30))
                {
                    Directory.Delete(dir, recursive: true);
                    _logger.LogInformation("Deleted expired artifact: {Path}", dir);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process artifact: {Path}", dir);
            }
        }
    }
}
