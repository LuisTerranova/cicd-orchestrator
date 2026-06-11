using System.Runtime.Versioning;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orchestrator.Runner.Configuration;
using Orchestrator.Runner.Container;
using Orchestrator.Runner.Execution;
using Orchestrator.Runner.Reconciliation;
using Orchestrator.Runner.Registration;
using Orchestrator.Runner.WebSocket;

namespace Orchestrator.Runner.Agent;

[SupportedOSPlatform("linux")]
public sealed class RunnerAgent : IHostedLifecycleService
{
    private readonly RunnerOptions _options;
    private readonly RunnerState _state;
    private readonly CredentialStore _credentialStore;
    private readonly RunnerRegistrar _registrar;
    private readonly ServerWebSocketClient _webSocket;
    private readonly Reconciliator _reconciliator;
    private readonly ContainerCleanupService _cleanup;
    private readonly ILogger<RunnerAgent> _logger;
    private string _runnerId = string.Empty;
    private CancellationTokenSource? _receiveCts;

    public RunnerAgent(
        RunnerOptions options,
        RunnerState state,
        CredentialStore credentialStore,
        RunnerRegistrar registrar,
        ServerWebSocketClient webSocket,
        Reconciliator reconciliator,
        ContainerCleanupService cleanup,
        ILogger<RunnerAgent> logger
    )
    {
        _options = options;
        _state = state;
        _credentialStore = credentialStore;
        _registrar = registrar;
        _webSocket = webSocket;
        _reconciliator = reconciliator;
        _cleanup = cleanup;
        _logger = logger;
    }

    public async Task StartingAsync(CancellationToken ct) { }

    public async Task StartedAsync(CancellationToken ct) { }

    public async Task StoppedAsync(CancellationToken ct) { }

    public async Task StartAsync(CancellationToken ct)
    {
        _logger.LogInformation(
            "Runner starting — name: {Name}, concurrency: {Concurrency}",
            _options.Name,
            _options.Concurrency
        );

        if (!string.IsNullOrEmpty(_options.RunnerId) && !string.IsNullOrEmpty(_options.RunnerSecret))
        {
            _runnerId = _options.RunnerId;
            await _credentialStore.SaveAsync(_options.RunnerId, _options.RunnerSecret);
            _logger.LogInformation("Using credentials from startup config for runner {RunnerId}", _runnerId);
        }
        else if (_credentialStore.Exists())
        {
            var creds = await _credentialStore.LoadAsync();
            _runnerId = creds.Value.RunnerId;
            _logger.LogInformation("Loaded existing credentials for runner {RunnerId}", _runnerId);
        }
        else
        {
            var token = _options.RegistrationToken;
            var (runnerId, secret) = await _registrar.RegisterAsync(token, ct);
            await _credentialStore.SaveAsync(runnerId, secret);
            _runnerId = runnerId;
        }

        await _webSocket.ConnectAsync(ct);
        await RunReconcileAndCleanupAsync(ct);

        _receiveCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _ = RunReceiveLoopAsync(_receiveCts.Token);

        _logger.LogInformation("Runner {RunnerId} ready. Awaiting jobs.", _runnerId);
    }

    public async Task StoppingAsync(CancellationToken ct)
    {
        _logger.LogInformation("Shutdown requested. Draining jobs...");
        _state.Draining = true;

        _receiveCts?.Cancel();

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await _state.WaitForActiveJobs(timeout.Token);

        try
        {
            await _webSocket.CloseAsync(
                System.Net.WebSockets.WebSocketCloseStatus.NormalClosure,
                "shutdown",
                ct
            );
        }
        catch
        {
            // Best-effort close
        }
    }

    public async Task StopAsync(CancellationToken ct)
    {
        await _cleanup.CleanAsync(_runnerId, ct);
    }

    private async Task RunReceiveLoopAsync(CancellationToken ct)
    {
        await _webSocket.RunReceiveLoopAsync(
            onMessage: async (msg, _) =>
            {
                _logger.LogDebug("WebSocket message received: {Msg}", msg);
            },
            onReconnect: async ct2 =>
            {
                await RunReconcileAndCleanupAsync(ct2);
            },
            ct
        );
    }

    private async Task RunReconcileAndCleanupAsync(CancellationToken ct)
    {
        var status = _state.ActiveJobIds.Length > 0 ? "busy" : "idle";
        await _reconciliator.ReconcileAsync(_runnerId, status, _state.ActiveJobIds, ct);
        var orphaned = await _cleanup.CleanAsync(_runnerId, ct);
        if (orphaned > 0)
            _logger.LogInformation("Cleaned {Count} orphaned containers", orphaned);
    }
}
