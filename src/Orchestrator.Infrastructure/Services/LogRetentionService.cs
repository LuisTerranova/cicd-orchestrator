using System.IO.Compression;
using System.IO.Compression;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orchestrator.Infrastructure.Configuration;

namespace Orchestrator.Infrastructure.Services;

public sealed class LogRetentionService : BackgroundService
{
    private readonly IOptions<StorageOptions> _storage;
    private readonly ILogger<LogRetentionService> _logger;

    public LogRetentionService(
        IOptions<StorageOptions> storage,
        ILogger<LogRetentionService> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LogRetentionService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRun = now.Date.AddDays(1).AddHours(2);
            var delay = nextRun - now;

            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, stoppingToken);

            if (stoppingToken.IsCancellationRequested)
                break;

            try
            {
                await RunRetentionCycleAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Log retention cycle failed");
            }
        }
    }

    private async Task RunRetentionCycleAsync(CancellationToken ct)
    {
        var logDir = _storage.Value.LogsPath;

        if (!Directory.Exists(logDir))
            return;

        var now = DateTime.UtcNow;
        var logFiles = Directory.GetFiles(logDir, "*.log");

        foreach (var filePath in logFiles)
        {
            var fileInfo = new FileInfo(filePath);
            var age = now - fileInfo.CreationTimeUtc;

            try
            {
                if (age > TimeSpan.FromDays(30))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Deleted expired log: {Path}", filePath);
                }
                else if (age > TimeSpan.FromDays(7))
                {
                    var gzPath = filePath + ".gz";
                    if (!File.Exists(gzPath))
                    {
                        await CompressLogAsync(filePath, gzPath, ct);
                        File.Delete(filePath);
                        _logger.LogInformation("Compressed warm log: {Path}", filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process log: {Path}", filePath);
            }
        }
    }

    private static async Task CompressLogAsync(string sourcePath, string targetPath, CancellationToken ct)
    {
        await using var sourceStream = File.OpenRead(sourcePath);
        await using var targetStream = File.Create(targetPath);
        await using var gzipStream = new GZipStream(targetStream, CompressionLevel.Optimal);
        await sourceStream.CopyToAsync(gzipStream, ct);
    }
}
