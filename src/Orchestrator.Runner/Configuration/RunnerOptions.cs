namespace Orchestrator.Runner.Configuration;

public sealed class RunnerOptions
{
    public string ServerUrl { get; init; } = "http://localhost:5000";
    public string CredentialsPath { get; init; } = "~/.orchestrator/credentials.json";
    public string EncryptionKey { get; init; } = string.Empty;

    public string[] Labels { get; init; } = [];
    public string Name { get; init; } = $"runner-{Environment.MachineName}";

    public string WorkspacePath { get; init; } = "/tmp/runner-workspaces";
    public int Concurrency { get; init; } = 1;
    public string ContainerRuntime { get; init; } = "podman";

    public TimeSpan HeartbeatInterval { get; init; } = TimeSpan.FromSeconds(10);
    public TimeSpan CleanupInterval { get; init; } = TimeSpan.FromMinutes(5);
    public TimeSpan ProgressInterval { get; init; } = TimeSpan.FromSeconds(30);
    public TimeSpan StepGracefulShutdown { get; init; } = TimeSpan.FromSeconds(10);
    public TimeSpan ContainerGracefulStop { get; init; } = TimeSpan.FromSeconds(10);

    public string LogLevel { get; init; } = "Information";
    public int LogUploadIntervalLines { get; init; } = 1000;
    public long LogMaxSizeBytes { get; init; } = 10L * 1024 * 1024;
}
