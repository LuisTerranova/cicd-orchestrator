using System.Text;
using System.Text.Json;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Infrastructure.Webhooks;

public class HttpClientWebhookDispatcher(HttpClient client) : IWebhookDispatcher
{
    public async Task DispatchAsync(string url, object payload, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content, ct);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException) { }
    }
}
