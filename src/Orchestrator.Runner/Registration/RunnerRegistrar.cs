using System.Net.Http.Json;
using System.Runtime.InteropServices;
using Orchestrator.Runner.Configuration;
using Orchestrator.Runner.Cli;

namespace Orchestrator.Runner.Registration;

public sealed class RunnerRegistrar
{
    private readonly HttpClient _http;
    private readonly RunnerOptions _options;

    public RunnerRegistrar(HttpClient http, RunnerOptions options)
    {
        _http = http;
        _options = options;
    }

    public async Task<(string RunnerId, string Secret)> RegisterAsync(string? token, CancellationToken ct)
    {
        var payload = new
        {
            token = token ?? string.Empty,
            runnerName = _options.Name,
            labels = _options.Labels,
            os = RuntimeInformation.OSDescription,
            arch = RuntimeInformation.OSArchitecture.ToString()
        };

        var maxRetries = 3;
        var delay = TimeSpan.FromSeconds(2);

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("/api/runners/register", payload, ct);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<RegisterResponse>(ct);
                return (result!.RunnerId, result.Secret);
            }
            catch (Exception) when (attempt < maxRetries && !ct.IsCancellationRequested)
            {
                await Task.Delay(delay, ct);
                delay *= 2;
            }
        }

        throw new InvalidOperationException("Failed to register runner after 3 attempts.");
    }

    private sealed record RegisterResponse(string RunnerId, string Secret);
}
