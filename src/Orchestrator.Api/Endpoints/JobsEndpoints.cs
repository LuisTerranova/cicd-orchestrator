using Orchestrator.Api.Extensions;
using Orchestrator.Application.Common;
using Orchestrator.Application.Jobs;

namespace Orchestrator.Api.Endpoints;

public class JobsEndpoints : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/jobs").WithTags("Jobs");

        group.MapPost("/{jobId:guid}/assign", AssignJobAsync);
        group.MapPost("/{jobId:guid}/cancel", CancelJobAsync);
        group.MapPost("/{jobId:guid}/complete", CompleteJobAsync);
    }

    private static async Task<IResult> AssignJobAsync(
        Guid jobId,
        JobAssignRequest request,
        ICommandHandler<AssignJobCommand> assignJobHandler,
        CancellationToken ct
    )
    {
        var command = new AssignJobCommand(jobId, request.RunnerId);
        await assignJobHandler.HandleAsync(command, ct);
        return Results.Ok();
    }

    private static async Task<IResult> CancelJobAsync(
        Guid jobId,
        JobCancelRequest request,
        ICommandHandler<CancelJobCommand> cancelJobHandler,
        CancellationToken ct
    )
    {
        var command = new CancelJobCommand(jobId, request.Reason);
        await cancelJobHandler.HandleAsync(command, ct);
        return Results.Ok();
    }

    private static async Task<IResult> CompleteJobAsync(
        Guid jobId,
        JobCompleteRequest request,
        ICommandHandler<CompleteJobCommand> completeJobHandler,
        CancellationToken ct
    )
    {
        var command = new CompleteJobCommand(jobId, request.ExitCode);
        await completeJobHandler.HandleAsync(command, ct);
        return Results.Ok();
    }
}
