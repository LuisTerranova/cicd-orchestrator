namespace Orchestrator.Domain;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
