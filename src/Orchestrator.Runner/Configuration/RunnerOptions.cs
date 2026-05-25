namespace Orchestrator.Runner.Configuration;

public sealed class RunnerOptions
{
    public string ServerUrl { get; set; } = "http://localhost:5000";
    public string CredentialsPath { get; set; } = "~/.orchestrator/credentials.json";
    public string EncryptionKey { get; set; } = string.Empty;

    public string[] Labels { get; set; } = [];
    public string Name { get; set; } = $"runner-{Environment.MachineName}";

    public string WorkspacePath { get; set; } = "/tmp/runner-workspaces";
    public int Concurrency { get; set; } = 1;
    public string ContainerRuntime { get; set; } = "podman";

    public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(10);
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan ProgressInterval { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan StepGracefulShutdown { get; set; } = TimeSpan.FromSeconds(10);
    public TimeSpan ContainerGracefulStop { get; set; } = TimeSpan.FromSeconds(10);

    public string LogLevel { get; set; } = "Information";
    public int LogUploadIntervalLines { get; set; } = 1000;
    public long LogMaxSizeBytes { get; set; } = 10L * 1024 * 1024;
}
