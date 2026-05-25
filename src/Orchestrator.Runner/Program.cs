using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orchestrator.Runner.Agent;
using Orchestrator.Runner.Artifacts;
using Orchestrator.Runner.Cli;
using Orchestrator.Runner.Configuration;
using Orchestrator.Runner.Container;
using Orchestrator.Runner.Execution;
using Orchestrator.Runner.Logging;
using Orchestrator.Runner.Messaging;
using Orchestrator.Runner.Monitoring;
using Orchestrator.Runner.Reconciliation;
using Orchestrator.Runner.Registration;
using Orchestrator.Runner.Secrets;
using Orchestrator.Runner.WebSocket;

var cli = new CliRootCommand();
var parseResult = cli.Invoke(args);
var configPath = parseResult.GetValue<string>("--config");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<RunnerOptions>();
        services.AddSingleton<RunnerOptionsValidator>();
        services.AddSingleton<RunnerState>();
        services.AddSingleton<SecretDecryptor>();
        services.AddSingleton<CredentialStore>();
        services.AddSingleton<RunnerRegistrar>();
        services.AddSingleton<ServerWebSocketClient>();
        services.AddSingleton<PodmanCli>();
        services.AddSingleton<ProcessInvoker>();
        services.AddSingleton<StepRunner>();
        services.AddSingleton<JobExecutor>();
        services.AddSingleton<LogCapturer>();
        services.AddSingleton<LogUploader>();
        services.AddSingleton<SecretMasker>();
        services.AddSingleton<HeartbeatService>();
        services.AddSingleton<ContainerCleanupService>();
        services.AddSingleton<ProgressReporter>();
        services.AddSingleton<Reconciliator>();
        services.AddSingleton<ArtifactUploader>();
        services.AddSingleton<RunnerAgent>();
        services.AddSingleton<JobConsumer>();
        services.AddSingleton<CancellationConsumer>();
        services.AddSingleton<JobResultPublisher>();

        MassTransitSetup.Configure(services);
        services.AddHttpClient();
    })
    .Build();

await host.Services.GetRequiredService<RunnerAgent>().StartAsync(CancellationToken.None);
await host.WaitForShutdownAsync();
