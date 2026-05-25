using Orchestrator.Api.Extensions;
using Orchestrator.Application.Jobs;

namespace Orchestrator.Api.Endpoints;

public static class JobsEndpoints
{
    public static void MapJobsEndpoints(this IEndpointRouteBuilder app)
    {
        // POST /api/jobs/{jobId:guid}/assign - Assign a job to a runner
        app.MapPost("/api/jobs/{jobId:guid}/assign", async (Guid jobId, JobAssignRequest request, HttpContext http, CancellationToken ct) =>
        {
            var handler = http.RequestServices.GetRequiredService<AssignJobHandler>();
            var command = new AssignJobCommand(jobId, request.RunnerId);
            await handler.HandleAsync(command, ct);
            return Results.Ok();
        });

        // POST /api/jobs/{jobId:guid}/cancel - Cancel a running job
        app.MapPost("/api/jobs/{jobId:guid}/cancel", async (Guid jobId, JobCancelRequest request, HttpContext http, CancellationToken ct) =>
        {
            var handler = http.RequestServices.GetRequiredService<CancelJobHandler>();
            var command = new CancelJobCommand(jobId, request.Reason);
            await handler.HandleAsync(command, ct);
            return Results.Ok();
        });

        // POST /api/jobs/{jobId:guid}/complete - Complete a job execution
        app.MapPost("/api/jobs/{jobId:guid}/complete", async (Guid jobId, JobCompleteRequest request, HttpContext http, CancellationToken ct) =>
        {
            var handler = http.RequestServices.GetRequiredService<CompleteJobHandler>();
            var command = new CompleteJobCommand(jobId, request.ExitCode);
            await handler.HandleAsync(command, ct);
            return Results.Ok();
        });
    }
}

