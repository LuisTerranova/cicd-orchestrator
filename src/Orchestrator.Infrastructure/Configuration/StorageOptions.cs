namespace Orchestrator.Infrastructure.Configuration;

public sealed record StorageOptions
{
    public const string SectionName = "Storage";

    public string LogsPath { get; init; } = "/data/logs";
    public string ArtifactsPath { get; init; } = "/data/artifacts";
}
