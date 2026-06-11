using System.Net.Http.Json;
using System.Text.Json;

namespace Orchestrator.Front.Services;

public class OrchestratorApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public OrchestratorApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<string?> LoginAsync(string username, string password)
    {
        var payload = new LoginRequestDto { Username = username, Password = password };
        var response = await _http.PostAsJsonAsync("/api/v1/auth/login", payload);
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(Json);
        if (result?.Token != null)
        {
            SetToken(result.Token);
            return result.Token;
        }
        return null;
    }

    public void SetToken(string token)
    {
        _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearToken()
    {
        _http.DefaultRequestHeaders.Authorization = null;
    }

    private sealed record LoginResponse(string Token);

    public async Task<PipelineResponse[]?> GetPipelinesAsync(int page = 1, int pageSize = 10)
    {
        var result = await _http.GetFromJsonAsync<PagedResponse<PipelineResponse[]>>(
            $"/api/v1/pipelines?page={page}&pageSize={pageSize}", Json);
        return result?.Data;
    }

    public async Task<PipelineResponse?> GetPipelineAsync(Guid id) =>
        await _http.GetFromJsonAsync<PipelineResponse>($"/api/v1/pipelines/{id}", Json);

    public async Task<Guid> CreatePipelineAsync(string name, string repo, string? branch, string? yamlPath)
    {
        var payload = new CreatePipelineRequestDto
        {
            Name = name,
            Repo = repo,
            Branch = branch ?? "main",
            YamlPath = yamlPath ?? ""
        };
        var response = await _http.PostAsJsonAsync("/api/v1/pipelines", payload);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<IdResponse>(Json);
        return result?.Id ?? Guid.Empty;
    }

    public async Task<PagedResponse<BuildDetailResponse[]>?> GetBuildsDetailAsync(
        Guid pipelineId, int page = 1, int pageSize = 20)
    {
        var response = await _http.GetAsync(
            $"/api/v1/pipelines/{pipelineId}/builds/detail?page={page}&pageSize={pageSize}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PagedResponse<BuildDetailResponse[]>>(Json);
    }

    public async Task<BuildResponse[]?> GetBuildsAsync(Guid pipelineId)
    {
        var response = await _http.GetAsync($"/api/v1/pipelines/{pipelineId}/builds");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BuildResponse[]>(Json);
    }

    public async Task<BuildDetailResponse?> GetBuildAsync(Guid id) =>
        await _http.GetFromJsonAsync<BuildDetailResponse>($"/api/v1/builds/{id}", Json);

    public async Task<Guid> TriggerBuildAsync(Guid pipelineId, string triggerEvent, string commitSha)
    {
        var payload = new TriggerBuildRequestDto
        {
            PipelineId = pipelineId,
            TriggerEvent = triggerEvent,
            CommitSha = commitSha,
            Actor = "web",
            Branch = "main"
        };
        var response = await _http.PostAsJsonAsync("/api/v1/builds", payload);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<IdResponse>(Json);
        return result?.Id ?? Guid.Empty;
    }

    public async Task CancelBuildAsync(Guid id)
    {
        var response = await _http.PostAsJsonAsync($"/api/v1/builds/{id}/cancel", new Dictionary<string, string>());
        response.EnsureSuccessStatusCode();
    }

    public async Task<RunnerResponse[]?> GetRunnersAsync(int page = 1, int pageSize = 10)
    {
        var result = await _http.GetFromJsonAsync<PagedResponse<RunnerResponse[]>>(
            $"/api/v1/runners?page={page}&pageSize={pageSize}", Json);
        return result?.Data;
    }

    public async Task<LogResponse?> GetLogAsync(Guid jobId) =>
        await _http.GetFromJsonAsync<LogResponse>($"/api/v1/builds/logs/{jobId}", Json);

    public async Task<string> GetLogContentAsync(Guid jobId, int offset = 0, int limit = 100)
    {
        var response = await _http.GetAsync($"/api/v1/builds/logs/{jobId}/content?offset={offset}&limit={limit}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task UpdatePipelineAsync(Guid id, string name, string repo, string branch, string yamlPath)
    {
        var payload = new UpdatePipelineRequestDto
        {
            Name = name,
            Repo = repo,
            Branch = branch,
            YamlPath = yamlPath
        };
        var response = await _http.PutAsJsonAsync($"/api/v1/pipelines/{id}", payload);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdatePipelineYamlAsync(Guid id, string yamlContent)
    {
        var payload = new UpdatePipelineYamlRequestDto { YamlContent = yamlContent };
        var response = await _http.PutAsJsonAsync($"/api/v1/pipelines/{id}/yaml", payload);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeletePipelineAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"/api/v1/pipelines/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<RegisterRunnerResponse?> RegisterRunnerAsync(string name, string[] labels, string os, string arch, string token)
    {
        var payload = new RegisterRunnerRequestDto
        {
            Name = name,
            Labels = labels,
            Os = os,
            Arch = arch,
            Token = token
        };
        var response = await _http.PostAsJsonAsync("/api/v1/runners/register", payload);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RegisterRunnerResponse>(Json);
    }

    private sealed record IdResponse(Guid Id);

    // Trim-safe DTO classes (parameterless constructors with properties)
    public sealed class LoginRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public sealed class CreatePipelineRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Repo { get; set; } = string.Empty;
        public string Branch { get; set; } = "main";
        public string YamlPath { get; set; } = string.Empty;
    }

    public sealed class TriggerBuildRequestDto
    {
        public Guid PipelineId { get; set; }
        public string TriggerEvent { get; set; } = string.Empty;
        public string CommitSha { get; set; } = string.Empty;
        public string Actor { get; set; } = "web";
        public string Branch { get; set; } = "main";
    }

    public sealed class UpdatePipelineRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Repo { get; set; } = string.Empty;
        public string Branch { get; set; } = "main";
        public string YamlPath { get; set; } = string.Empty;
    }

    public sealed class UpdatePipelineYamlRequestDto
    {
        public string YamlContent { get; set; } = string.Empty;
    }

    public sealed class RegisterRunnerRequestDto
    {
        public string? Name { get; set; }
        public string[]? Labels { get; set; }
        public string? Os { get; set; }
        public string? Arch { get; set; }
        public string? Token { get; set; }
    }
}
