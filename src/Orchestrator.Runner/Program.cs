#pragma warning disable CA1416

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

if (parseResult.HasOption("--version"))
{
    Console.WriteLine("Orchestrator.Runner 1.0.0");
    return;
}

var configPath = parseResult.HasOption("--config")
    ? parseResult.GetValue<string>("--config")
    : "~/.orchestrator/config.yml";

var loader = new ConfigurationLoader();
var options = loader.Load(configPath, parseResult);

using var loggerFactory = LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(
    Enum.TryParse<LogLevel>(options.LogLevel, out var level) ? level : LogLevel.Information));
var validator = new RunnerOptionsValidator(loggerFactory.CreateLogger<RunnerOptionsValidator>());

var errors = validator.Validate(options);
if (errors.Count > 0)
{
    foreach (var error in errors)
        Console.Error.WriteLine($"FATAL: {error}");
    Environment.Exit(1);
}

if (parseResult.HasOption("--dry-run"))
{
    Console.WriteLine("Configuration valid. Dry-run complete.");
    return;
}

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton(options);
        services.AddSingleton(loggerFactory);
        services.AddSingleton<RunnerState>();
        services.AddSingleton<SecretDecryptor>();
        services.AddSingleton<CredentialStore>();
        services.AddSingleton<RunnerRegistrar>();
        services.AddSingleton<ServerWebSocketClient>();
        services.AddSingleton<PodmanCli>();
        services.AddSingleton<ProcessInvoker>();
        services.AddSingleton<TempScriptWriter>();
        services.AddSingleton<NativeStepRunner>();
        services.AddSingleton<ContainerStepRunner>();
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

        MassTransitSetup.Configure(services, options);
        services.AddHttpClient();
    })
    .Build();

await host.Services.GetRequiredService<RunnerAgent>().StartAsync(CancellationToken.None);
await host.WaitForShutdownAsync();
