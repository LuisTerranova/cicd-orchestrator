using Microsoft.Extensions.Logging;
using Orchestrator.Domain;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Infrastructure.Services;

/// <summary>
/// A simple implementation of IDomainEventDispatcher that logs events to ILogger.
/// </summary>
public sealed class DomainEventDispatcher(ILogger<DomainEventDispatcher> logger)
    : IDomainEventDispatcher
{
    public async Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> events,
        CancellationToken ct = default
    )
    {
        foreach (var e in events)
        {
            logger.LogInformation("Domain event: {EventType}", e.GetType().Name);
        }

        await Task.CompletedTask;
    }
}
