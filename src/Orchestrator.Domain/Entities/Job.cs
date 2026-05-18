using Orchestrator.Domain.Events;
using Orchestrator.Domain.Exceptions;
using Orchestrator.Domain.ValueObjects;

namespace Orchestrator.Domain.Entities;

public class Job : Entity
{
    public Guid BuildId { get; private set; }
    public required string StageName { get; private set; }
    public JobStatus Status { get; private set; }
    public Guid? RunnerId { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int Version { get; private set; }

    private Job() { }

    public static Job Create(Guid buildId, string stageName, int version = 1) { }

    public void AssignTo(Runner runner) { }

    public void Complete(int exitCode) { }

    public void Cancel(string reason) { }

    public void Skip() { }

    public void Queue() { }
}
