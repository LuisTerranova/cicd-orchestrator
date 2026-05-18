using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Infrastructure.Webhooks;

public class HttpClientWebhookDispatcher : IWebhookDispatcher
{
    private readonly HttpClient _httpClient;

    public HttpClientWebhookDispatcher(HttpClient httpClient) { }

    public Task DispatchAsync(string url, object payload, CancellationToken ct = default)
        => throw new NotImplementedException();
}
