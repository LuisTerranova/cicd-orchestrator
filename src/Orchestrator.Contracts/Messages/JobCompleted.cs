namespace Orchestrator.Contracts.Messages;

public sealed record JobCompleted(
    int Version,
    Guid JobId,
    string RunnerId,
    string Status,
    int ExitCode,
    DateTime StartedAt,
    DateTime CompletedAt,
    TimeSpan Duration,
    JobStepResult[] Steps,
    ArtifactInfo[] Artifacts,
    string? ErrorMessage
);

public sealed record JobStepResult(
    string Name,
    string Status,
    int ExitCode,
    TimeSpan Duration
);

public sealed record ArtifactInfo(
    string Name,
    string Path,
    long SizeBytes
);
