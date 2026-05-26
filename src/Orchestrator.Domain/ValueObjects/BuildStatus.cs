namespace Orchestrator.Domain.ValueObjects;

public enum BuildStatus
{
    Queued,
    Running,
    Passed,
    Failed,
    Cancelled,
    PassedWithWarnings,
}
