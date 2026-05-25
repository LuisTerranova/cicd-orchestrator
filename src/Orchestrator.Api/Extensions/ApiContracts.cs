using System;

namespace Orchestrator.Api.Extensions;

// Health Endpoints DTOs
public sealed record HealthResponse(string Status, DateTime Timestamp);

// Pipeline Endpoints DTOs
public sealed record PipelineResponse(Guid Id, string Name, string Repo, string Branch, DateTime CreatedAt);
public sealed record PipelineCreateRequest(string Name, string Repo, string? Branch, string? YamlPath);

// Build Endpoints DTOs
public sealed record BuildResponse(Guid Id, Guid PipelineId, string Status, DateTime CreatedAt, DateTime? CompletedAt);
public sealed record BuildTriggerRequest(Guid PipelineId, string TriggerEvent, string CommitSha, int Priority = 0);

// Runner Endpoints DTOs
public sealed record RunnerResponse(Guid Id, string Name, string Status, string[] Labels, DateTime LastSeen);
public sealed record RegisterRunnerResponse(Guid RunnerId, string Secret);
public sealed record ReconcileRequest(string RunnerStatus, Guid[] ActiveJobs);
public sealed record ReconcileResponse(OrphanedJob[] OrphanedJobs, string ServerStatus);
public sealed record OrphanedJob(Guid JobId, string Reason);

// Job Endpoints DTOs
public sealed record JobAssignRequest(Guid RunnerId);
public sealed record JobCancelRequest(string Reason);
public sealed record JobCompleteRequest(int ExitCode);

// Log Endpoints DTOs
public sealed record LogResponse(Guid Id, Guid JobId, string FilePath, int LineCount, long SizeBytes, DateTime CreatedAt);
