using Orchestrator.Application.Common;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Jobs;

public sealed class CancelJobHandler(IJobRepository jobs, IDomainEventDispatcher eventDispatcher)
    : ICommandHandler<CancelJobCommand>
{
    public async Task HandleAsync(CancelJobCommand command, CancellationToken ct = default)
    {
        var job =
            await jobs.GetByIdAsync(command.JobId, ct)
            ?? throw new InvalidOperationException($"Job {command.JobId} not found");

        job.Cancel(command.Reason);
        await jobs.UpdateAsync(job, ct);
        await eventDispatcher.DispatchAsync(job.DomainEvents, ct);
        job.ClearDomainEvents();
    }
}
