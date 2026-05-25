using System.Text;
using System.Text.Json;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Infrastructure.Webhooks;

public class HttpClientWebhookDispatcher : IWebhookDispatcher
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HttpClientWebhookDispatcher(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task DispatchAsync(string url, object payload, CancellationToken ct = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content, ct);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException)
        {
        }
    }
}
