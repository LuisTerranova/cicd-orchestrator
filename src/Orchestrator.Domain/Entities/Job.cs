using Orchestrator.Domain.Events;
using Orchestrator.Domain.ValueObjects;

namespace Orchestrator.Domain.Entities;

public class Job : Entity
{
    public Guid BuildId { get; private set; }
    public string StageName { get; private set; } = string.Empty;
    public JobStatus Status { get; private set; }
    public Guid? RunnerId { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int Version { get; private set; }

    private Job() { }

    public static Job Create(Guid buildId, string stageName, int version = 1)
        => throw new NotImplementedException();

    public void AssignTo(Runner runner)
        => throw new NotImplementedException();

    public void Complete(int exitCode)
        => throw new NotImplementedException();

    public void Cancel(string reason)
        => throw new NotImplementedException();

    public void Skip()
        => throw new NotImplementedException();

    public void Queue()
        => throw new NotImplementedException();
}
