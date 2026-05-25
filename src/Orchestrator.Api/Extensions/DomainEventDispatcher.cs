using Microsoft.Extensions.Logging;
using Orchestrator.Application.Common;
using Orchestrator.Domain;

namespace Orchestrator.Api.Extensions;

/// <summary>
/// A simple implementation of IDomainEventDispatcher that logs events to ILogger.
/// </summary>
public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(ILogger<DomainEventDispatcher> logger)
    {
        _logger = logger;
    }

    public async Task DispatchAsync(IReadOnlyCollection<IDomainEvent> events, CancellationToken ct = default)
    {
        foreach (var e in events)
        {
            _logger.LogInformation("Domain event: {EventType}", e.GetType().Name);
        }

        await Task.CompletedTask;
    }
}
