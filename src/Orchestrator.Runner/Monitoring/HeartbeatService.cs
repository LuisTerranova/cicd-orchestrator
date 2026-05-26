using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orchestrator.Runner.Configuration;
using Orchestrator.Runner.WebSocket;

namespace Orchestrator.Runner.Monitoring;

public sealed class HeartbeatService : BackgroundService
{
    private readonly ServerWebSocketClient _ws;
    private readonly TimeSpan _interval;
    private readonly ILogger<HeartbeatService> _logger;

    public HeartbeatService(
        ServerWebSocketClient ws,
        RunnerOptions options,
        ILogger<HeartbeatService> logger
    )
    {
        _ws = ws;
        _interval = options.HeartbeatInterval;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _ws.SendPingAsync(stoppingToken);
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Heartbeat failed: {Ex}", ex.Message);
                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
