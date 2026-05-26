using Orchestrator.Application.Common;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Jobs;

public sealed class CompleteJobHandler(IJobRepository jobs, IDomainEventDispatcher eventDispatcher)
    : ICommandHandler<CompleteJobCommand>
{
    public async Task HandleAsync(CompleteJobCommand command, CancellationToken ct = default)
    {
        var job =
            await jobs.GetByIdAsync(command.JobId, ct)
            ?? throw new InvalidOperationException($"Job {command.JobId} not found");

        job.Complete(command.ExitCode);
        await eventDispatcher.DispatchAsync(job.DomainEvents, ct);
        job.ClearDomainEvents();
    }
}
