using Orchestrator.Api.Extensions;
using Orchestrator.Application.Builds;

namespace Orchestrator.Api.Endpoints;

public static class BuildsEndpoints
{
    public static void MapBuildsEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /api/builds/{id:guid} - Retrieve build details by ID
        app.MapGet("/api/builds/{id:guid}", async (Guid id, HttpContext http, CancellationToken ct) =>
        {
            var query = http.RequestServices.GetRequiredService<GetBuildByIdQuery>();
            var build = await query.HandleAsync(id, ct);
            if (build == null)
                return Results.NotFound();

            var response = new BuildResponse(build.Id, build.PipelineId, build.Status.ToString(), build.CreatedAt, build.CompletedAt);
            return Results.Ok(response);
        });

        // GET /api/pipelines/{pipelineId:guid}/builds - Retrieve all builds for a pipeline
        app.MapGet("/api/pipelines/{pipelineId:guid}/builds", async (Guid pipelineId, HttpContext http, CancellationToken ct) =>
        {
            var query = http.RequestServices.GetRequiredService<GetBuildsByPipelineIdQuery>();
            var builds = await query.HandleAsync(pipelineId, ct);
            var response = builds.Select(b => new BuildResponse(b.Id, b.PipelineId, b.Status.ToString(), b.CreatedAt, b.CompletedAt)).ToArray();
            return Results.Ok(response);
        });

        // POST /api/builds - Trigger a new build for a pipeline
        app.MapPost("/api/builds", async (BuildTriggerRequest request, HttpContext http, CancellationToken ct) =>
        {
            var handler = http.RequestServices.GetRequiredService<TriggerBuildHandler>();
            var command = new TriggerBuildCommand(request.PipelineId, request.TriggerEvent, request.CommitSha, request.Priority);
            var id = await handler.HandleAsync(command, ct);
            return Results.Created($"/api/builds/{id}", new { id });
        });
    }
}

