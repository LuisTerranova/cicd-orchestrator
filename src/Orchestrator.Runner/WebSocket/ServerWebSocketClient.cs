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
    private readonly SemaphoreSlim _connectLock = new(1, 1);

    private const int MaxBackoffSec = 30;

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

    public async Task ConnectAsync(CancellationToken ct)
    {
        var wsUri = new UriBuilder(_options.ServerUrl) { Scheme = "ws", Path = "/ws/runner" }.Uri;
        var creds = await _credentials.LoadAsync();
        var token = creds?.Secret ?? string.Empty;

        var delay = TimeSpan.FromSeconds(1);

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await _connectLock.WaitAsync(ct);
                try
                {
                    _ws?.Dispose();
                    _ws = new ClientWebSocket();
                    _ws.Options.SetRequestHeader("Authorization", $"Bearer {token}");
                    await _ws.ConnectAsync(wsUri, ct);
                }
                finally
                {
                    _connectLock.Release();
                }

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
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, MaxBackoffSec));
            }
        }
    }

    // Runs a receive loop that keeps the connection alive.
    // If the connection drops, automatically reconnects with backoff.
    // The onMessage callback is invoked for each received message.
    public async Task RunReceiveLoopAsync(
        Func<string, CancellationToken, Task> onMessage,
        Func<CancellationToken, Task> onReconnect,
        CancellationToken ct
    )
    {
        var buffer = new byte[4096];

        while (!ct.IsCancellationRequested)
        {
            if (_ws?.State != WebSocketState.Open)
            {
                _logger.LogWarning("WebSocket disconnected. Reconnecting...");
                await ConnectAsync(ct);
                await onReconnect(ct);
            }

            try
            {
                var result = await _ws!.ReceiveAsync(new ArraySegment<byte>(buffer), ct);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("WebSocket closed by server. Reconnecting...");
                    await ConnectAsync(ct);
                    await onReconnect(ct);
                    continue;
                }

                var text = Encoding.UTF8.GetString(buffer, 0, result.Count);
                await onMessage(text, ct);
            }
            catch when (!ct.IsCancellationRequested)
            {
                _logger.LogWarning("WebSocket receive error. Reconnecting...");
                await ConnectAsync(ct);
                await onReconnect(ct);
            }
        }
    }

    public async Task SendMessageAsync(string message, CancellationToken ct = default)
    {
        if (_ws?.State != WebSocketState.Open)
            return;

        var bytes = Encoding.UTF8.GetBytes(message);
        await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, ct);
    }

    public async Task SendPingAsync(CancellationToken ct)
    {
        if (_ws?.State != WebSocketState.Open)
            return;

        var ping = new byte[] { 0x9 };
        await _ws.SendAsync(new ArraySegment<byte>(ping), WebSocketMessageType.Binary, true, ct);
    }

    public async Task CloseAsync(
        WebSocketCloseStatus status,
        string description,
        CancellationToken ct
    )
    {
        if (_ws?.State == WebSocketState.Open)
            await _ws.CloseAsync(status, description, ct);

        _ws?.Dispose();
        _ws = null;
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync(WebSocketCloseStatus.NormalClosure, "Dispose", CancellationToken.None);
        GC.SuppressFinalize(this);
    }
}
