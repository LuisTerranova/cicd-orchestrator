using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Orchestrator.Runner.Configuration;

public sealed class ConfigurationLoader
{
    public RunnerOptions Load(string configPath, Cli.CliParseResult? cli = null)
    {
        // Start with defaults (already baked into RunnerOptions)
        var options = new RunnerOptions();

        // 2. Overlay YAML config
        if (File.Exists(configPath))
        {
            var yaml = File.ReadAllText(configPath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            var yamlConfig = deserializer.Deserialize<YamlConfig>(yaml);

            if (yamlConfig.Server is not null)
            {
                options.ServerUrl = yamlConfig.Server.Url ?? options.ServerUrl;
                options.CredentialsPath =
                    yamlConfig.Server.CredentialsPath ?? options.CredentialsPath;
                options.EncryptionKey = yamlConfig.Server.EncryptionKey ?? options.EncryptionKey;
            }

            if (yamlConfig.Runner is not null)
            {
                options.Name = yamlConfig.Runner.Name ?? options.Name;
                if (yamlConfig.Runner.Labels is { Length: > 0 })
                    options.Labels = yamlConfig.Runner.Labels;
                options.WorkspacePath = yamlConfig.Runner.WorkspacePath ?? options.WorkspacePath;
                options.Concurrency = yamlConfig.Runner.Concurrency ?? options.Concurrency;
                options.ContainerRuntime =
                    yamlConfig.Runner.ContainerRuntime ?? options.ContainerRuntime;
                if (yamlConfig.Runner.HeartbeatInterval.HasValue)
                    options.HeartbeatInterval = yamlConfig.Runner.HeartbeatInterval.Value;
                if (yamlConfig.Runner.CleanupInterval.HasValue)
                    options.CleanupInterval = yamlConfig.Runner.CleanupInterval.Value;
            }
        }

        // 3. Overlay env vars
        if (GetEnv("RUNNER_SERVER_URL") is { } envServerUrl)
            options.ServerUrl = envServerUrl;
        if (GetEnv("RUNNER_CREDENTIALS_PATH") is { } envCredPath)
            options.CredentialsPath = envCredPath;
        if (GetEnv("RUNNER_ENCRYPTION_KEY") is { } envKey)
            options.EncryptionKey = envKey;
        if (GetEnv("RUNNER_NAME") is { } envName)
            options.Name = envName;
        if (GetEnv("RUNNER_LABELS") is { } envLabels)
            options.Labels = envLabels.Split(
                ',',
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
            );
        if (GetEnv("RUNNER_WORKSPACE_PATH") is { } envWs)
            options.WorkspacePath = envWs;
        if (GetEnv("RUNNER_CONCURRENCY") is { } envConc && int.TryParse(envConc, out var conc))
            options.Concurrency = conc;
        if (GetEnv("RUNNER_CONTAINER_RUNTIME") is { } envRt)
            options.ContainerRuntime = envRt;
        if (
            GetEnv("RUNNER_HEARTBEAT_INTERVAL") is { } envHbi
            && TimeSpan.TryParse(envHbi, out var hbi)
        )
            options.HeartbeatInterval = hbi;

        if (GetEnv("RUNNER_RABBITMQ_HOST") is { } envRmqHost)
            options.RabbitMqHost = envRmqHost;
        if (GetEnv("RUNNER_RABBITMQ_USER") is { } envRmqUser)
            options.RabbitMqUser = envRmqUser;
        if (GetEnv("RUNNER_RABBITMQ_PASS") is { } envRmqPass)
            options.RabbitMqPass = envRmqPass;
        if (GetEnv("RUNNER_REGISTRATION_TOKEN") is { } envRegToken)
            options.RegistrationToken = envRegToken;
        if (GetEnv("RUNNER_ID") is { } envRunnerId)
            options.RunnerId = envRunnerId;
        if (GetEnv("RUNNER_SECRET") is { } envRunnerSecret)
            options.RunnerSecret = envRunnerSecret;

        // 4. Overlay CLI flags
        if (cli is not null)
        {
            if (cli.HasOption("--server-url"))
                options.ServerUrl = cli.GetValue<string>("--server-url") ?? options.ServerUrl;
            if (cli.HasOption("--concurrency"))
                options.Concurrency = cli.GetValue<int>("--concurrency");
            if (cli.HasOption("--labels"))
                options.Labels = cli.GetValue<string[]>("--labels") ?? options.Labels;
            if (cli.HasOption("--registration-token"))
                options.RegistrationToken = cli.GetValue<string>("--registration-token") ?? options.RegistrationToken;
            if (cli.HasOption("--runner-id"))
                options.RunnerId = cli.GetValue<string>("--runner-id") ?? options.RunnerId;
            if (cli.HasOption("--runner-secret"))
                options.RunnerSecret = cli.GetValue<string>("--runner-secret") ?? options.RunnerSecret;
        }

        return options;
    }

    private static string? GetEnv(string name) =>
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } value ? value : null;

    // YAML binding model — mirrors appsettings.yml structure
    private sealed class YamlConfig
    {
        public YamlServerSection? Server { get; set; }
        public YamlRunnerSection? Runner { get; set; }
    }

    private sealed class YamlServerSection
    {
        public string? Url { get; set; }
        public string? CredentialsPath { get; set; }
        public string? EncryptionKey { get; set; }
    }

    private sealed class YamlRunnerSection
    {
        public string? Name { get; set; }
        public string[]? Labels { get; set; }
        public string? WorkspacePath { get; set; }
        public int? Concurrency { get; set; }
        public string? ContainerRuntime { get; set; }
        public TimeSpan? HeartbeatInterval { get; set; }
        public TimeSpan? CleanupInterval { get; set; }
    }
}
