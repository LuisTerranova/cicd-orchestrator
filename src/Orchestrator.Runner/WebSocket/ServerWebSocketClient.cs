using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Orchestrator.Runner.Configuration;
using Orchestrator.Runner.Registration;

namespace Orchestrator.Runner.WebSocket;

public sealed class ServerWebSocketClient : IAsyncDisposable
{
    private readonly RunnerOptions _options;
    private readonly CredentialStore _credentials;
    private readonly ILogger<ServerWebSocketClient> _logger;
    private ClientWebSocket? _ws;

    public ServerWebSocketClient(
        RunnerOptions options,
        CredentialStore credentials,
        ILogger<ServerWebSocketClient> logger
    )
    {
        _options = options;
        _credentials = credentials;
        _logger = logger;
    }

    // Connects to the server WebSocket endpoint with exponential backoff.
    // The Authorization header carries the runner's secret token for server-side
    // authentication. Retry delay doubles from 1s up to a 30s cap.
    public async Task ConnectAsync(CancellationToken ct)
    {
        // Convert http(s) to ws(s) for the WebSocket URI.
        var wsUri = new UriBuilder(_options.ServerUrl) { Scheme = "ws", Path = "/ws/runner" }.Uri;
        var creds = await _credentials.LoadAsync();
        var token = creds?.Secret ?? string.Empty;

        var delay = TimeSpan.FromSeconds(1);
        const int maxDelaySec = 30;

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                _ws?.Dispose();
                _ws = new ClientWebSocket();
                _ws.Options.SetRequestHeader("Authorization", $"Bearer {token}");

                await _ws.ConnectAsync(wsUri, ct);
                _logger.LogInformation("WebSocket connected to {Uri}", wsUri);
                return;
            }
            catch when (!ct.IsCancellationRequested)
            {
                _logger.LogWarning(
                    "WebSocket connect attempt {Attempt} failed, retrying in {Delay}s",
                    attempt,
                    delay.TotalSeconds
                );
                await Task.Delay(delay, ct);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, maxDelaySec));
            }
        }
    }

    // Sends a UTF-8 text message over the open WebSocket.
    public async Task SendMessageAsync(string message, CancellationToken ct = default)
    {
        if (_ws?.State != WebSocketState.Open)
            return;

        var bytes = Encoding.UTF8.GetBytes(message);
        await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, ct);
    }

    // Sends a binary PING frame to keep the connection alive.
    public async Task SendPingAsync(CancellationToken ct)
    {
        if (_ws?.State != WebSocketState.Open)
            return;

        var ping = new byte[] { 0x9 };
        await _ws.SendAsync(new ArraySegment<byte>(ping), WebSocketMessageType.Binary, true, ct);
    }

    // Closes the WebSocket gracefully if open, then disposes the underlying instance.
    public async Task CloseAsync(
        WebSocketCloseStatus status,
        string description,
        CancellationToken ct
    )
    {
        if (_ws?.State == WebSocketState.Open)
        {
            await _ws.CloseAsync(status, description, ct);
        }

        _ws?.Dispose();
        _ws = null;
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync(WebSocketCloseStatus.NormalClosure, "Dispose", CancellationToken.None);
        GC.SuppressFinalize(this);
    }
}
