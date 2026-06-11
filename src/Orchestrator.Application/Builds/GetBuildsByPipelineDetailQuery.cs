using Microsoft.Extensions.Logging;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Builds;

public sealed class GetBuildsByPipelineDetailQuery
{
    private readonly IBuildRepository _builds;
    private readonly IPipelineRepository _pipelines;
    private readonly ILogger<GetBuildsByPipelineDetailQuery> _logger;

    public GetBuildsByPipelineDetailQuery(
        IBuildRepository builds,
        IPipelineRepository pipelines,
        ILogger<GetBuildsByPipelineDetailQuery> logger)
    {
        _builds = builds;
        _pipelines = pipelines;
        _logger = logger;
    }

    public async Task<Domain.Common.PagedResult<BuildDetail>> HandleAsync(
        Guid pipelineId,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var pipeline = await _pipelines.GetByIdAsync(pipelineId, ct);
        var pipelineName = pipeline?.Name ?? "unknown";

        var paged = await _builds.GetPagedByPipelineIdAsync(pipelineId, page, pageSize, ct);

        var items = paged.Items
            .Select(b => new BuildDetail(
                b.Id,
                b.PipelineId,
                pipelineName,
                b.Status.ToString(),
                b.TriggerEvent,
                b.CommitSha,
                b.CreatedAt,
                b.CompletedAt,
                b.Priority,
                b.Jobs
                    .Select(j => new JobDetail(
                        j.Id,
                        j.StageName,
                        j.Status.ToString(),
                        j.RunnerId,
                        j.StartedAt,
                        j.CompletedAt
                    ))
                    .ToArray()
            ))
            .ToList();

        return new Domain.Common.PagedResult<BuildDetail>(items, paged.TotalCount);
    }
}

public sealed record BuildDetail(
    Guid Id,
    Guid PipelineId,
    string PipelineName,
    string Status,
    string TriggerEvent,
    string CommitSha,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    int Priority,
    JobDetail[] Jobs
);

public sealed record JobDetail(
    Guid Id,
    string StageName,
    string Status,
    Guid? RunnerId,
    DateTime? StartedAt,
    DateTime? CompletedAt
);
