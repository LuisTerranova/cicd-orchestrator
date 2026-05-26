using System.Net.Http.Headers;
using System.Net.Http.Json;
using Orchestrator.Runner.Registration;

namespace Orchestrator.Runner.Reconciliation;

public sealed class Reconciliator
{
    private readonly HttpClient _http;
    private readonly CredentialStore _credentials;

    public Reconciliator(HttpClient httpClient, CredentialStore credentials)
    {
        _http = httpClient;
        _credentials = credentials;
    }

    // POSTs the runner's current status and active job set to the server.
    // The server can detect orphaned assignments or state mismatches and respond accordingly.
    // Uses bearer token authentication from stored credentials.
    public async Task ReconcileAsync(
        string runnerId,
        string status,
        Guid[] activeJobs,
        CancellationToken ct
    )
    {
        var payload = new { status, activeJobs };

        var creds = await _credentials.LoadAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/runners/{runnerId}/reconcile")
        {
            Content = JsonContent.Create(payload),
        };

        if (creds.HasValue && creds.Value.Secret is { Length: > 0 })
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                creds.Value.Secret
            );
        }

        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }
}
