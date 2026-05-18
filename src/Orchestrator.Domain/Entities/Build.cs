using Orchestrator.Domain.Events;
using Orchestrator.Domain.Exceptions;
using Orchestrator.Domain.ValueObjects;

namespace Orchestrator.Domain.Entities;

public class Build : Entity
{
    public Guid PipelineId { get; private set; }
    public required string TriggerEvent { get; private set; }
    public string CommitSha { get; private set; } = string.Empty;
    public BuildStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int Priority { get; private set; }

    private readonly List<Job> _jobs = [];
    public IReadOnlyCollection<Job> Jobs => _jobs.AsReadOnly();

    private Build() { }

    public static Build Create(Guid pipelineId, string triggerEvent, string commitSha, int priority = 0) { }

    public void Start() { }

    public void Complete(BuildStatus finalStatus) { }

    public void Cancel() { }

    public Job AddJob(string stageName) { }
}
