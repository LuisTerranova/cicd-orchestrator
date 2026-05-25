using Orchestrator.Api.Extensions;
using Orchestrator.Application.Runners;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Domain.ValueObjects;

namespace Orchestrator.Api.Endpoints;

public static class RunnersEndpoints
{
    public static void MapRunnersEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /api/runners - Retrieve all runners
        app.MapGet("/api/runners", async (HttpContext http, CancellationToken ct) =>
        {
            var query = http.RequestServices.GetRequiredService<GetAllRunnersQuery>();
            var runners = await query.HandleAsync(ct);
            var response = runners.Select(r => new RunnerResponse(r.Id, r.Name, r.Status.ToString(), r.Labels, r.LastSeen)).ToArray();
            return Results.Ok(response);
        });

        // GET /api/runners/{id:guid} - Retrieve runner by ID
        app.MapGet("/api/runners/{id:guid}", async (Guid id, HttpContext http, CancellationToken ct) =>
        {
            var query = http.RequestServices.GetRequiredService<GetRunnerByIdQuery>();
            var runner = await query.HandleAsync(id, ct);
            if (runner == null)
                return Results.NotFound();

            var response = new RunnerResponse(runner.Id, runner.Name, runner.Status.ToString(), runner.Labels, runner.LastSeen);
            return Results.Ok(response);
        });

        // POST /api/runners/register - Register a new runner agent
        app.MapPost("/api/runners/register", async (RegisterRunnerCommand command, HttpContext http, CancellationToken ct) =>
        {
            var handler = http.RequestServices.GetRequiredService<RegisterRunnerHandler>();
            var runnerId = await handler.HandleAsync(command, ct);
            
            var tokenGenerator = http.RequestServices.GetRequiredService<IRunnerTokenGenerator>();
            var secret = tokenGenerator.GenerateToken(runnerId);

            return Results.Created($"/api/runners/{runnerId}", new RegisterRunnerResponse(runnerId, secret));
        });

        // POST /api/runners/{id:guid}/reconcile - Reconcile runner active jobs and heartbeat status
        app.MapPost("/api/runners/{id:guid}/reconcile", async (Guid id, ReconcileRequest request, HttpContext http, CancellationToken ct) =>
        {
            var jobRepository = http.RequestServices.GetRequiredService<IJobRepository>();
            var runningJobs = await jobRepository.GetByStatusAsync(JobStatus.Running, ct);
            var runnerJobs = runningJobs.Where(j => j.RunnerId == id).ToList();

            // Detect orphaned jobs (running on server but not active on runner)
            var orphanedJobs = runnerJobs
                .Where(j => !request.ActiveJobs.Contains(j.Id))
                .Select(j => new OrphanedJob(j.Id, "Job is marked as running on server but is not active on runner"))
                .ToArray();

            // Update runner heartbeat and status
            var runnerRepository = http.RequestServices.GetRequiredService<IRunnerRepository>();
            var unitOfWork = http.RequestServices.GetRequiredService<IUnitOfWork>();
            var runner = await runnerRepository.GetByIdAsync(id, ct);
            if (runner != null)
            {
                runner.Heartbeat();

                if (Enum.TryParse<RunnerStatus>(request.RunnerStatus, true, out var reportedStatus))
                {
                    if (reportedStatus == RunnerStatus.Idle)
                    {
                        if (runner.Status == RunnerStatus.Busy)
                            runner.GoIdle();
                        else if (runner.Status == RunnerStatus.Offline || runner.Status == RunnerStatus.Disconnected)
                            runner.Register();
                    }
                    else if (reportedStatus == RunnerStatus.Busy)
                    {
                        if (runner.Status == RunnerStatus.Idle)
                            runner.GoBusy();
                        else if (runner.Status == RunnerStatus.Offline || runner.Status == RunnerStatus.Disconnected)
                        {
                            runner.Register();
                            runner.GoBusy();
                        }
                    }
                }

                await runnerRepository.UpdateAsync(runner, ct);
                await unitOfWork.SaveChangesAsync(ct);
            }

            return Results.Ok(new ReconcileResponse(orphanedJobs, "active"));
        });
    }
}

