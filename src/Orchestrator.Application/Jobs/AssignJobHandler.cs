using Orchestrator.Application.Common;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Jobs;

public sealed class AssignJobHandler(
    IJobRepository jobs,
    IRunnerRepository runners,
    IDomainEventDispatcher eventDispatcher
) : ICommandHandler<AssignJobCommand>
{
    public async Task HandleAsync(AssignJobCommand command, CancellationToken ct = default)
    {
        var job =
            await jobs.GetByIdAsync(command.JobId, ct)
            ?? throw new InvalidOperationException($"Job {command.JobId} not found");
        var runner =
            await runners.GetByIdAsync(command.RunnerId, ct)
            ?? throw new InvalidOperationException($"Runner {command.RunnerId} not found");

        job.AssignTo(runner);
        runner.GoBusy();
        await jobs.UpdateAsync(job, ct);
        await runners.UpdateAsync(runner, ct);
        await eventDispatcher.DispatchAsync(job.DomainEvents, ct);
        job.ClearDomainEvents();
    }
}
