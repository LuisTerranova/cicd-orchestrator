namespace Orchestrator.Domain.ValueObjects;

public enum JobStatus
{
    Pending,
    Queued,
    Running,
    Passed,
    Failed,
    Cancelled,
    Skipped,
}
