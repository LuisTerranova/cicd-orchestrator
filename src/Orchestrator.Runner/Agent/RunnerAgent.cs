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
        // Step 1: Register or load existing credentials
        _logger.LogInformation(
            "Runner starting — name: {Name}, concurrency: {Concurrency}",
            _options.Name,
            _options.Concurrency
        );

        if (_credentialStore.Exists())
        {
            var creds = await _credentialStore.LoadAsync();
            _runnerId = creds.Value.RunnerId;
            _logger.LogInformation("Loaded existing credentials for runner {RunnerId}", _runnerId);
        }
        else
        {
            var token = Environment.GetEnvironmentVariable("RUNNER_REGISTRATION_TOKEN");
            var (runnerId, secret) = await _registrar.RegisterAsync(token, ct);
            await _credentialStore.SaveAsync(runnerId, secret);
            _runnerId = runnerId;
            _logger.LogInformation("Registered new runner {RunnerId}", _runnerId);
        }

        // Step 2: Establish WebSocket connection
        await _webSocket.ConnectAsync(ct);

        // Step 3: Reconcile with server
        await _reconciliator.ReconcileAsync(_runnerId, "idle", [], ct);

        // Step 4: Cleanup orphaned containers from previous runs
        var orphaned = await _cleanup.CleanAsync(_runnerId, ct);
        if (orphaned > 0)
            _logger.LogInformation("Cleaned {Count} orphaned containers", orphaned);

        _logger.LogInformation("Runner {RunnerId} ready. Awaiting jobs.", _runnerId);
    }

    public async Task StoppingAsync(CancellationToken ct)
    {
        _logger.LogInformation("Shutdown requested. Draining jobs...");
        _state.Draining = true;

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
}
