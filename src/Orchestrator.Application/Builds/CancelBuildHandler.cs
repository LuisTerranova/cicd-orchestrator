using Microsoft.Extensions.Logging;
using Orchestrator.Application.Common;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Domain.ValueObjects;

namespace Orchestrator.Application.Builds;

public sealed class CancelBuildHandler(
    IBuildRepository builds,
    IJobRepository jobs,
    IDomainEventDispatcher eventDispatcher,
    ILogger<CancelBuildHandler> logger
) : ICommandHandler<CancelBuildCommand>
{
    public async Task HandleAsync(CancelBuildCommand command, CancellationToken ct = default)
    {
        var build =
            await builds.GetByIdAsync(command.BuildId, ct)
            ?? throw new InvalidOperationException($"Build {command.BuildId} not found");

        if (build.Status is Domain.ValueObjects.BuildStatus.Passed
            or Domain.ValueObjects.BuildStatus.Failed
            or Domain.ValueObjects.BuildStatus.Cancelled)
        {
            logger.LogWarning(
                "Build {BuildId} already in terminal state {Status}. Cannot cancel.",
                command.BuildId,
                build.Status
            );
            return;
        }

        build.Cancel();
        logger.LogInformation(
            "Build {BuildId} cancelled. Cascading cancellation to {JobCount} jobs.",
            command.BuildId,
            build.Jobs.Count
        );

        var allEvents = new List<Domain.IDomainEvent>();
        allEvents.AddRange(build.DomainEvents);

        foreach (var job in build.Jobs)
        {
            if (job.Status is JobStatus.Pending or JobStatus.Queued)
            {
                job.Cancel(command.Reason);
                allEvents.AddRange(job.DomainEvents);
                job.ClearDomainEvents();
                logger.LogInformation("Job {JobId} ({Stage}) cancelled.", job.Id, job.StageName);
            }
        }

        await eventDispatcher.DispatchAsync(allEvents, ct);
        build.ClearDomainEvents();
        await builds.UpdateAsync(build, ct);
    }
}
