using Orchestrator.Domain.Events;
using Orchestrator.Domain.Exceptions;
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
    {
        if (pipelineId == Guid.Empty)
            throw new DomainException("PipelineId cannot be empty.");
        if (string.IsNullOrWhiteSpace(triggerEvent))
            throw new DomainException("Trigger event cannot be empty.");
        if (string.IsNullOrWhiteSpace(commitSha))
            throw new DomainException("Commit SHA cannot be empty.");

        return new Build
        {
            Id = Guid.NewGuid(),
            PipelineId = pipelineId,
            TriggerEvent = triggerEvent,
            CommitSha = commitSha,
            Status = BuildStatus.Queued,
            CreatedAt = DateTime.UtcNow,
            Priority = priority
        };
    }

    public void Start()
    {
        if (Status != BuildStatus.Queued)
            throw new DomainException("Only queued builds can be started.");

        Status = BuildStatus.Running;
        AddDomainEvent(new BuildStartedEvent(Id, PipelineId));
    }

    public void Complete(BuildStatus finalStatus)
    {
        if (finalStatus != BuildStatus.Passed && finalStatus != BuildStatus.Failed && finalStatus != BuildStatus.PassedWithWarnings)
            throw new DomainException("Build can only complete with Passed, Failed, or PassedWithWarnings status.");

        Status = finalStatus;
        CompletedAt = DateTime.UtcNow;
        AddDomainEvent(new BuildCompletedEvent(Id, Status));
    }

    public void Cancel()
    {
        Status = BuildStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
    }

    public Job AddJob(string stageName)
    {
        var job = Job.Create(Id, stageName);
        _jobs.Add(job);
        return job;
    }
}
