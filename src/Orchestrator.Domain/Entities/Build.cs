using Orchestrator.Domain.Events;
using Orchestrator.Domain.ValueObjects;

namespace Orchestrator.Domain.Entities;

public class Build : Entity
{
    public Guid PipelineId { get; private set; }
    public string TriggerEvent { get; private set; } = string.Empty;
    public string CommitSha { get; private set; } = string.Empty;
    public BuildStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int Priority { get; private set; }

    private readonly List<Job> _jobs = [];
    public IReadOnlyCollection<Job> Jobs => _jobs.AsReadOnly();

    private Build() { }

    public static Build Create(Guid pipelineId, string triggerEvent, string commitSha, int priority = 0)
        => throw new NotImplementedException();

    public void Start()
        => throw new NotImplementedException();

    public void Complete(BuildStatus finalStatus)
        => throw new NotImplementedException();

    public void Cancel()
        => throw new NotImplementedException();

    public Job AddJob(string stageName)
        => throw new NotImplementedException();
}
