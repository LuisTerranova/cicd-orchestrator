using Orchestrator.Api.Extensions;
using Orchestrator.Application.Builds;
using Orchestrator.Application.Common;

namespace Orchestrator.Api.Endpoints;

public class BuildsEndpoints : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/builds").WithTags("Builds");

        group.MapGet("/{id:guid}", GetBuildByIdAsync);
        group.MapPost("/", TriggerBuildAsync);

        app.MapGet("/api/pipelines/{pipelineId:guid}/builds", GetBuildsByPipelineAsync)
            .WithTags("Builds");
    }

    private static async Task<IResult> GetBuildByIdAsync(
        Guid id,
        GetBuildByIdQuery getBuildByIdQuery,
        CancellationToken ct
    )
    {
        var build = await getBuildByIdQuery.HandleAsync(id, ct);
        if (build == null)
            return Results.NotFound();

        var response = new BuildResponse(
            build.Id,
            build.PipelineId,
            build.Status.ToString(),
            build.CreatedAt,
            build.CompletedAt
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
            request.Priority
        );
        var id = await triggerBuildHandler.HandleAsync(command, ct);
        return Results.Created($"/api/builds/{id}", new { id });
    }
}
