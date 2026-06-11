namespace Orchestrator.Contracts.Messages;

public enum JobResultStatus
{
    Passed,
    Failed,
    Cancelled,
}

public sealed record JobCompleted(
    int Version,
    Guid JobId,
    string RunnerId,
    JobResultStatus Status,
    int ExitCode,
    DateTime StartedAt,
    DateTime CompletedAt,
    TimeSpan Duration,
    JobStepResult[] Steps,
    ArtifactInfo[] Artifacts,
    string? ErrorMessage,
    string MessageId = "",
    DateTime Timestamp = default,
    string TraceId = ""
);

public sealed record JobStepResult(string Name, string Status, int ExitCode, TimeSpan Duration);

public sealed record ArtifactInfo(string Name, string Path, long SizeBytes);
