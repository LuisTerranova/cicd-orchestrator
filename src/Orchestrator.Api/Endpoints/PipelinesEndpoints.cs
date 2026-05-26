using Orchestrator.Api.Extensions;
using Orchestrator.Application.Common;
using Orchestrator.Application.Pipelines;

namespace Orchestrator.Api.Endpoints;

public class PipelinesEndpoints : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pipelines").WithTags("Pipelines");

        group.MapGet("/", GetAllPipelinesAsync);
        group.MapGet("/{id:guid}", GetPipelineByIdAsync);
        group.MapPost("/", CreatePipelineAsync);
    }

    private static async Task<IResult> GetAllPipelinesAsync(
        GetAllPipelinesQuery getAllPipelinesQuery,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default
    )
    {
        var result = await getAllPipelinesQuery.HandleAsync(page, pageSize, ct);
        var dtos = result
            .Items.Select(p => new PipelineResponse(p.Id, p.Name, p.Repo, p.Branch, p.CreatedAt))
            .ToArray();
        return Results.Ok(
            new PagedResponse<PipelineResponse[]>(dtos, result.TotalCount, page, pageSize)
        );
    }

    private static async Task<IResult> GetPipelineByIdAsync(
        Guid id,
        GetPipelineByIdQuery getPipelineByIdQuery,
        CancellationToken ct
    )
    {
        var pipeline = await getPipelineByIdQuery.HandleAsync(id, ct);
        if (pipeline == null)
            return Results.NotFound();

        var response = new PipelineResponse(
            pipeline.Id,
            pipeline.Name,
            pipeline.Repo,
            pipeline.Branch,
            pipeline.CreatedAt
        );
        return Results.Ok(response);
    }

    private static async Task<IResult> CreatePipelineAsync(
        PipelineCreateRequest request,
        ICommandHandler<CreatePipelineCommand, Guid> createPipelineHandler,
        CancellationToken ct
    )
    {
        var command = new CreatePipelineCommand(
            request.Name,
            request.Repo,
            request.Branch ?? "main",
            request.YamlPath ?? ""
        );
        var id = await createPipelineHandler.HandleAsync(command, ct);
        return Results.Created($"/api/pipelines/{id}", new { id });
    }
}
