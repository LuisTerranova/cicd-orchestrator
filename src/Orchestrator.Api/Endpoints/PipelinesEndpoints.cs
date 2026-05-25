using Orchestrator.Api.Extensions;
using Orchestrator.Application.Pipelines;

namespace Orchestrator.Api.Endpoints;

public static class PipelinesEndpoints
{
    public static void MapPipelinesEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /api/pipelines - Retrieve all pipelines
        app.MapGet("/api/pipelines", async (HttpContext http, CancellationToken ct) =>
        {
            var query = http.RequestServices.GetRequiredService<GetAllPipelinesQuery>();
            var pipelines = await query.HandleAsync(ct);
            var response = pipelines.Select(p => new PipelineResponse(p.Id, p.Name, p.Repo, p.Branch, p.CreatedAt)).ToArray();
            return Results.Ok(response);
        });

        // GET /api/pipelines/{id:guid} - Retrieve pipeline by ID
        app.MapGet("/api/pipelines/{id:guid}", async (Guid id, HttpContext http, CancellationToken ct) =>
        {
            var query = http.RequestServices.GetRequiredService<GetPipelineByIdQuery>();
            var pipeline = await query.HandleAsync(id, ct);
            if (pipeline == null)
                return Results.NotFound();

            var response = new PipelineResponse(pipeline.Id, pipeline.Name, pipeline.Repo, pipeline.Branch, pipeline.CreatedAt);
            return Results.Ok(response);
        });

        // POST /api/pipelines - Create a new pipeline
        app.MapPost("/api/pipelines", async (PipelineCreateRequest request, HttpContext http, CancellationToken ct) =>
        {
            var handler = http.RequestServices.GetRequiredService<CreatePipelineHandler>();
            var command = new CreatePipelineCommand(request.Name, request.Repo, request.Branch ?? "main", request.YamlPath ?? "");
            var id = await handler.HandleAsync(command, ct);
            return Results.Created($"/api/pipelines/{id}", new { id });
        });
    }
}

