namespace Orchestrator.Front.Services;

public sealed record PipelineResponse(
    Guid Id, string Name, string Repo, string Branch, string YamlPath,
    DateTime CreatedAt, bool HasYaml, string? YamlContent = null
);

public sealed record RegisterRunnerRequest(
    string Name,
    string[] Labels,
    string Os,
    string Arch
);

public sealed record RegisterRunnerResponse(
    Guid RunnerId,
    string Secret
);

public sealed record BuildResponse(
    Guid Id, Guid PipelineId, string Status,
    DateTime CreatedAt, DateTime? CompletedAt
);

public sealed record BuildDetailResponse(
    Guid Id, Guid PipelineId, string PipelineName,
    string Status, string TriggerEvent, string CommitSha,
    DateTime CreatedAt, DateTime? CompletedAt,
    int Priority, JobSummary[] Jobs
);

public sealed record JobSummary(
    Guid Id, string StageName, string Status,
    Guid? RunnerId, DateTime? StartedAt, DateTime? CompletedAt
);

public sealed record RunnerResponse(
    Guid Id, string Name, string Status,
    string[] Labels, DateTime LastSeen
);

public sealed record LogResponse(
    Guid Id, Guid JobId, string FilePath,
    int LineCount, long SizeBytes, DateTime CreatedAt
);

public sealed record PagedResponse<TData>
{
    public TData? Data { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
