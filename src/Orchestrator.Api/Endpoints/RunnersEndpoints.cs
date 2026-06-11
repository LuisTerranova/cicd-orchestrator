using Orchestrator.Api.Extensions;
using Orchestrator.Application.Common;
using Orchestrator.Application.Runners;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Domain.ValueObjects;
using Orchestrator.Infrastructure.Persistence;

namespace Orchestrator.Api.Endpoints;

public class RunnersEndpoints : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/runners").WithTags("Runners");

        group.MapGet("/", GetAllRunnersAsync);
        group.MapGet("/{id:guid}", GetRunnerByIdAsync);
        group.MapPost("/register", RegisterRunnerAsync);
        group.MapPost("/{id:guid}/reconcile", ReconcileRunnerAsync);
    }

    private static async Task<IResult> GetAllRunnersAsync(
        GetAllRunnersQuery query,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default
    )
    {
        var result = await query.HandleAsync(page, pageSize, ct);
        var dtos = result
            .Items.Select(r => new RunnerResponse(
                r.Id,
                r.Name,
                r.Status.ToString(),
                r.Labels,
                r.LastSeen
            ))
            .ToArray();
        return Results.Ok(
            new PagedResponse<RunnerResponse[]>(dtos, result.TotalCount, page, pageSize)
        );
    }

    private static async Task<IResult> GetRunnerByIdAsync(
        Guid id,
        GetRunnerByIdQuery query,
        CancellationToken ct
    )
    {
        var runner = await query.HandleAsync(id, ct);
        if (runner == null)
            return Results.NotFound();

        var response = new RunnerResponse(
            runner.Id,
            runner.Name,
            runner.Status.ToString(),
            runner.Labels,
            runner.LastSeen
        );
        return Results.Ok(response);
    }

    private static async Task<IResult> RegisterRunnerAsync(
        RegisterRunnerRequest request,
        ICommandHandler<RegisterRunnerCommand, Guid> handler,
        IRunnerTokenGenerator tokenGenerator,
        Microsoft.Extensions.Configuration.IConfiguration configuration,
        CancellationToken ct
    )
    {
        var expectedToken = Environment.GetEnvironmentVariable("RUNNER_REGISTRATION_TOKEN")
            ?? configuration["Auth:RegistrationToken"]
            ?? "dev-token";

        var providedToken = request.Token ?? "";
        if (providedToken != expectedToken)
        {
            return Results.Json(new { error = "Unauthorized token" }, statusCode: StatusCodes.Status401Unauthorized);
        }

        var name = request.Name ?? request.RunnerName ?? "unknown-runner";
        var command = new RegisterRunnerCommand(
            name,
            request.Labels ?? Array.Empty<string>(),
            request.Os ?? "unknown",
            request.Arch ?? "unknown"
        );
        var runnerId = await handler.HandleAsync(command, ct);
        var secret = tokenGenerator.GenerateToken(runnerId);

        return Results.Created(
            $"/api/v1/runners/{runnerId}",
            new RegisterRunnerResponse(runnerId, secret)
        );
    }

    private static async Task<IResult> ReconcileRunnerAsync(
        Guid id,
        ReconcileRequest request,
        IJobRepository jobRepository,
        IRunnerRepository runnerRepository,
        OrchestratorDbContext dbContext,
        CancellationToken ct
    )
    {
        var runningJobs = await jobRepository.GetByStatusAsync(JobStatus.Running, ct);
        var runnerJobs = runningJobs.Where(j => j.RunnerId == id).ToList();

        // Detect orphaned jobs (running on server but not active on runner)
        var orphanedJobs = runnerJobs
            .Where(j => !request.ActiveJobs.Contains(j.Id))
            .Select(j => new OrphanedJob(
                j.Id,
                "Job is marked as running on server but is not active on runner"
            ))
            .ToArray();

        // Update runner heartbeat and status
        var runner = await runnerRepository.GetByIdAsync(id, ct);
        if (runner == null)
            return Results.Ok(new ReconcileResponse(orphanedJobs, "active"));

        runner.Heartbeat();

        // Reconcile and transition runner status based on what the runner agent reports vs its current database status
        if (Enum.TryParse<RunnerStatus>(request.RunnerStatus, true, out var reportedStatus))
        {
            switch (reportedStatus, runner.Status)
            {
                // Runner reports it is Idle, but database says Busy -> Go Idle
                case (RunnerStatus.Idle, RunnerStatus.Busy):
                    runner.GoIdle();
                    break;

                // Runner reports Idle, but database says Offline/Disconnected -> Re-register (reconnect)
                case (RunnerStatus.Idle, RunnerStatus.Offline or RunnerStatus.Disconnected):
                    runner.Register();
                    break;

                // Runner reports Busy, but database says Idle -> Transition to Busy
                case (RunnerStatus.Busy, RunnerStatus.Idle):
                    runner.GoBusy();
                    break;

                // Runner reports Busy, but database says Offline/Disconnected -> Re-register first, then go Busy
                case (RunnerStatus.Busy, RunnerStatus.Offline or RunnerStatus.Disconnected):
                    runner.Register();
                    runner.GoBusy();
                    break;
            }
        }

        await runnerRepository.UpdateAsync(runner, ct);
        await dbContext.SaveChangesAsync(ct);

        return Results.Ok(new ReconcileResponse(orphanedJobs, "active"));
    }
}
