using System.Net.WebSockets;

namespace Orchestrator.Runner.WebSocket;

public sealed class ServerWebSocketClient
{
    public async Task ConnectAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task SendPingAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task CloseAsync(WebSocketCloseStatus status, string description, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
