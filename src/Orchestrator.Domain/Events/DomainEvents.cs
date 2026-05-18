using Orchestrator.Domain.ValueObjects;

namespace Orchestrator.Domain.Events;

public sealed record JobQueuedEvent(Guid JobId, Guid BuildId, int Version) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record JobAssignedEvent(Guid JobId, Guid RunnerId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record JobCompletedEvent(Guid JobId, JobStatus Status, int ExitCode) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record JobCancelledEvent(Guid JobId, string Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record BuildStartedEvent(Guid BuildId, Guid PipelineId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record BuildCompletedEvent(Guid BuildId, BuildStatus Status) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record RunnerRegisteredEvent(Guid RunnerId, string Name) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
