using Orchestrator.Domain.Events;
using Orchestrator.Domain.Exceptions;
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
    public int ExitCode { get; private set; }
    public int Version { get; private set; }

    private Job() { }

    public static Job Create(Guid buildId, string stageName, int version = 1)
    {
        if (buildId == Guid.Empty)
            throw new DomainException("BuildId cannot be empty.");
        if (string.IsNullOrWhiteSpace(stageName))
            throw new DomainException("Stage name cannot be empty.");

        return new Job
        {
            Id = Guid.NewGuid(),
            BuildId = buildId,
            StageName = stageName,
            Status = JobStatus.Pending,
            Version = version,
        };
    }

    public void AssignTo(Runner runner)
    {
        if (runner is null)
            throw new DomainException("Runner cannot be null.");
        if (Status != JobStatus.Pending)
            throw new DomainException("Only pending jobs can be assigned to a runner.");

        RunnerId = runner.Id;
        Status = JobStatus.Queued;
        AddDomainEvent(new JobAssignedEvent(Id, runner.Id));
    }

    public void Complete(int exitCode)
    {
        if (Status != JobStatus.Running && Status != JobStatus.Queued)
            throw new DomainException("Only running or queued jobs can be completed.");

        ExitCode = exitCode;
        Status = exitCode == 0 ? JobStatus.Passed : JobStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        AddDomainEvent(new JobCompletedEvent(Id, Status, exitCode));
    }

    public void Cancel(string reason)
    {
        if (Status is JobStatus.Passed or JobStatus.Failed or JobStatus.Cancelled or JobStatus.Skipped)
            throw new DomainException("Cannot cancel a job that has already completed.");

        Status = JobStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
        AddDomainEvent(new JobCancelledEvent(Id, reason));
    }

    public void Skip()
    {
        if (Status != JobStatus.Pending && Status != JobStatus.Queued)
            throw new DomainException("Only pending or queued jobs can be skipped.");

        Status = JobStatus.Skipped;
    }

    public void Queue()
    {
        if (Status != JobStatus.Pending)
            throw new DomainException("Only pending jobs can be queued.");

        Status = JobStatus.Queued;
        AddDomainEvent(new JobQueuedEvent(Id, BuildId, Version));
    }

    public void Start()
    {
        if (Status != JobStatus.Queued)
            throw new DomainException("Only queued jobs can be started.");

        Status = JobStatus.Running;
        StartedAt = DateTime.UtcNow;
    }
}
