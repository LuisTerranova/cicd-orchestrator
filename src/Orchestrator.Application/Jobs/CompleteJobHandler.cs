using Orchestrator.Application.Common;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Jobs;

public sealed class CompleteJobHandler
{
    private readonly IJobRepository _jobs;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public CompleteJobHandler(
        IJobRepository jobs,
        IUnitOfWork unitOfWork,
        IDomainEventDispatcher eventDispatcher)
    {
        _jobs = jobs;
        _unitOfWork = unitOfWork;
        _eventDispatcher = eventDispatcher;
    }

    public async Task HandleAsync(CompleteJobCommand command, CancellationToken ct = default)
    {
        var job = await _jobs.GetByIdAsync(command.JobId, ct)
            ?? throw new InvalidOperationException($"Job {command.JobId} not found");

        job.Complete(command.ExitCode);
        await _eventDispatcher.DispatchAsync(job.DomainEvents, ct);
        job.ClearDomainEvents();
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
