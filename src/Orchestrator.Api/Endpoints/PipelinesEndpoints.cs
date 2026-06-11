using Orchestrator.Api.Extensions;
using Orchestrator.Application.Builds;
using Orchestrator.Application.Common;
using Orchestrator.Application.Pipelines;

namespace Orchestrator.Api.Endpoints;

public class PipelinesEndpoints : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/pipelines").WithTags("Pipelines");

        group.MapGet("/", GetAllPipelinesAsync);
        group.MapGet("/{id:guid}", GetPipelineByIdAsync);
        group.MapPost("/", CreatePipelineAsync);
        group.MapPut("/{id:guid}", UpdatePipelineAsync);
        group.MapPut("/{id:guid}/yaml", UpdatePipelineYamlAsync);
        group.MapDelete("/{id:guid}", DeletePipelineAsync);
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
            .Items.Select(p => new PipelineResponse(
                p.Id,
                p.Name,
                p.Repo,
                p.Branch,
                p.YamlPath,
                p.CreatedAt,
                !string.IsNullOrEmpty(p.YamlContent),
                null
            ))
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
            pipeline.YamlPath,
            pipeline.CreatedAt,
            !string.IsNullOrEmpty(pipeline.YamlContent),
            pipeline.YamlContent
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
        return Results.Created($"/api/v1/pipelines/{id}", new { id });
    }

    private static async Task<IResult> UpdatePipelineAsync(
        Guid id,
        PipelineUpdateRequest request,
        ICommandHandler<UpdatePipelineCommand> updateHandler,
        CancellationToken ct
    )
    {
        var command = new UpdatePipelineCommand(
            id,
            request.Name,
            request.Repo,
            request.Branch ?? "main",
            request.YamlPath ?? ""
        );
        await updateHandler.HandleAsync(command, ct);
        return Results.Ok(new { message = "Pipeline updated successfully." });
    }

    private static async Task<IResult> UpdatePipelineYamlAsync(
        Guid id,
        PipelineYamlUpdateRequest request,
        ICommandHandler<UpdatePipelineYamlCommand> updateYamlHandler,
        CancellationToken ct
    )
    {
        var command = new UpdatePipelineYamlCommand(id, request.YamlContent);
        await updateYamlHandler.HandleAsync(command, ct);
        return Results.Ok(new { message = "Pipeline YAML updated." });
    }

    private static async Task<IResult> DeletePipelineAsync(
        Guid id,
        ICommandHandler<DeletePipelineCommand> deleteHandler,
        CancellationToken ct
    )
    {
        var command = new DeletePipelineCommand(id);
        await deleteHandler.HandleAsync(command, ct);
        return Results.Ok(new { message = "Pipeline deleted successfully." });
    }
}
