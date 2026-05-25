using Microsoft.Extensions.Hosting;

namespace Orchestrator.Runner.Monitoring;

public sealed class HeartbeatService : BackgroundService
{
    public HeartbeatService(WebSocket.ServerWebSocketClient ws, TimeSpan interval)
    {
        throw new NotImplementedException();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}
