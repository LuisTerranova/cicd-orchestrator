using Orchestrator.Api.Extensions;
using Orchestrator.Application.Builds;
using Orchestrator.Application.Common;
using Orchestrator.Application.Pipelines;

namespace Orchestrator.Api.Endpoints;

public class BuildsEndpoints : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/builds").WithTags("Builds");

        group.MapGet("/{id:guid}", GetBuildByIdAsync);
        group.MapPost("/", TriggerBuildAsync);
        group.MapPost("/{id:guid}/cancel", CancelBuildAsync);

        app.MapGet("/api/v1/pipelines/{pipelineId:guid}/builds", GetBuildsByPipelineAsync)
            .WithTags("Builds");
        app.MapGet("/api/v1/pipelines/{pipelineId:guid}/builds/detail", GetBuildsByPipelineDetailAsync)
            .WithTags("Builds");
    }

    private static async Task<IResult> GetBuildByIdAsync(
        Guid id,
        GetBuildByIdQuery getBuildByIdQuery,
        GetPipelineByIdQuery getPipelineByIdQuery,
        CancellationToken ct
    )
    {
        var build = await getBuildByIdQuery.HandleAsync(id, ct);
        if (build == null)
            return Results.NotFound();

        var pipeline = await getPipelineByIdQuery.HandleAsync(build.PipelineId, ct);

        var response = new BuildDetailResponse(
            build.Id,
            build.PipelineId,
            pipeline?.Name ?? "",
            build.Status.ToString(),
            build.TriggerEvent,
            build.CommitSha,
            build.CreatedAt,
            build.CompletedAt,
            build.Priority,
            build.Jobs
                .Select(j => new JobSummary(
                    j.Id,
                    j.StageName,
                    j.Status.ToString(),
                    j.RunnerId,
                    j.StartedAt,
                    j.CompletedAt
                ))
                .ToArray()
        );
        return Results.Ok(response);
    }

    private static async Task<IResult> GetBuildsByPipelineAsync(
        Guid pipelineId,
        GetBuildsByPipelineIdQuery getBuildsByPipelineIdQuery,
        CancellationToken ct
    )
    {
        var builds = await getBuildsByPipelineIdQuery.HandleAsync(pipelineId, ct);
        var response = builds
            .Select(b => new BuildResponse(
                b.Id,
                b.PipelineId,
                b.Status.ToString(),
                b.CreatedAt,
                b.CompletedAt
            ))
            .ToArray();
        return Results.Ok(response);
    }

    private static async Task<IResult> GetBuildsByPipelineDetailAsync(
        Guid pipelineId,
        GetBuildsByPipelineDetailQuery query,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default
    )
    {
        var result = await query.HandleAsync(pipelineId, page, pageSize, ct);
        var items = result.Items
            .Select(b => new BuildDetailResponse(
                b.Id,
                b.PipelineId,
                b.PipelineName,
                b.Status,
                b.TriggerEvent,
                b.CommitSha,
                b.CreatedAt,
                b.CompletedAt,
                b.Priority,
                b.Jobs.Select(j => new JobSummary(
                    j.Id, j.StageName, j.Status,
                    j.RunnerId, j.StartedAt, j.CompletedAt
                )).ToArray()
            ))
            .ToArray();
        return Results.Ok(new PagedResponse<BuildDetailResponse[]>(items, result.TotalCount, page, pageSize));
    }

    private static async Task<IResult> TriggerBuildAsync(
        BuildTriggerRequest request,
        ICommandHandler<TriggerBuildCommand, Guid> triggerBuildHandler,
        CancellationToken ct
    )
    {
        var command = new TriggerBuildCommand(
            request.PipelineId,
            request.TriggerEvent,
            request.CommitSha,
            request.Actor,
            request.Branch,
            request.Priority
        );
        var id = await triggerBuildHandler.HandleAsync(command, ct);

        if (id == Guid.Empty)
            return Results.Ok(new { message = "Build was filtered by trigger configuration." });

        return Results.Created($"/api/v1/builds/{id}", new { id });
    }

    private static async Task<IResult> CancelBuildAsync(
        Guid id,
        BuildCancelRequest? request,
        ICommandHandler<CancelBuildCommand> cancelBuildHandler,
        CancellationToken ct
    )
    {
        var command = new CancelBuildCommand(id, request?.Reason ?? "user_requested");
        await cancelBuildHandler.HandleAsync(command, ct);
        return Results.Accepted($"/api/v1/builds/{id}");
    }
}
