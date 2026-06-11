namespace Orchestrator.Api.Extensions;

// Health Endpoints DTOs
public sealed record HealthResponse(string Status, DateTime Timestamp);

// Pipeline Endpoints DTOs
public sealed record PipelineResponse(
    Guid Id,
    string Name,
    string Repo,
    string Branch,
    string YamlPath,
    DateTime CreatedAt,
    bool HasYaml,
    string? YamlContent = null
);

public sealed record PipelineCreateRequest(
    string Name,
    string Repo,
    string? Branch,
    string? YamlPath
);

public sealed record PipelineUpdateRequest(
    string Name,
    string Repo,
    string? Branch,
    string? YamlPath
);

// Build Endpoints DTOs
public sealed record BuildResponse(
    Guid Id,
    Guid PipelineId,
    string Status,
    DateTime CreatedAt,
    DateTime? CompletedAt
);

public sealed record BuildDetailResponse(
    Guid Id,
    Guid PipelineId,
    string PipelineName,
    string Status,
    string TriggerEvent,
    string CommitSha,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    int Priority,
    JobSummary[] Jobs
);

public sealed record JobSummary(
    Guid Id,
    string StageName,
    string Status,
    Guid? RunnerId,
    DateTime? StartedAt,
    DateTime? CompletedAt
);

public sealed record BuildTriggerRequest(
    Guid PipelineId,
    string TriggerEvent,
    string CommitSha,
    string Actor = "system",
    string Branch = "main",
    int Priority = 0
);

public sealed record BuildCancelRequest(string? Reason);

public sealed record PipelineYamlUpdateRequest(string YamlContent);

// Runner Endpoints DTOs
public sealed record RunnerResponse(
    Guid Id,
    string Name,
    string Status,
    string[] Labels,
    DateTime LastSeen
);

public sealed record RegisterRunnerRequest(
    string? Name,
    string? RunnerName,
    string[]? Labels,
    string? Os,
    string? Arch,
    string? Token = null
);

public sealed record RegisterRunnerResponse(Guid RunnerId, string Secret);

public sealed record ReconcileRequest(string RunnerStatus, Guid[] ActiveJobs);

public sealed record ReconcileResponse(OrphanedJob[] OrphanedJobs, string ServerStatus);

public sealed record OrphanedJob(Guid JobId, string Reason);

// Job Endpoints DTOs
public sealed record JobAssignRequest(Guid RunnerId);

public sealed record JobCancelRequest(string Reason);

public sealed record JobCompleteRequest(int ExitCode);

// Log Endpoints DTOs
public sealed record LogResponse(
    Guid Id,
    Guid JobId,
    string FilePath,
    int LineCount,
    long SizeBytes,
    DateTime CreatedAt
);

// Webhook Endpoints DTOs
public sealed record WebhookRequest(string Payload, string Signature, string Secret);

// Auth DTOs
public sealed record AuthTokenResponse(string Token, Guid RunnerId, DateTime ExpiresAt);

public sealed record ErrorResponse(string Code, string Message);

public class PagedResponse<TData>
{
    public PagedResponse() { }

    [System.Text.Json.Serialization.JsonConstructor]
    public PagedResponse(TData data, int totalCount, int currentPage = 1, int pageSize = 10)
    {
        Data = data;
        TotalCount = totalCount;
        CurrentPage = currentPage;
        PageSize = pageSize;
    }

    public TData? Data { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
}
