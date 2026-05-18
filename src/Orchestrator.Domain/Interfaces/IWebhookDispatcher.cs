namespace Orchestrator.Domain.Interfaces;

public interface IWebhookDispatcher
{
    Task DispatchAsync(string url, object payload, CancellationToken ct = default);
}
