using Orchestrator.Domain;

namespace Orchestrator.Application.Common;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> events, CancellationToken ct = default);
}
