using Orchestrator.Application.Common;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Jobs;

public sealed class AssignJobHandler
{
    private readonly IJobRepository _jobs;
    private readonly IRunnerRepository _runners;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public AssignJobHandler(
        IJobRepository jobs,
        IRunnerRepository runners,
        IUnitOfWork unitOfWork,
        IDomainEventDispatcher eventDispatcher)
    {
        _jobs = jobs;
        _runners = runners;
        _unitOfWork = unitOfWork;
        _eventDispatcher = eventDispatcher;
    }

    public async Task HandleAsync(AssignJobCommand command, CancellationToken ct = default)
    {
        var job = await _jobs.GetByIdAsync(command.JobId, ct)
            ?? throw new InvalidOperationException($"Job {command.JobId} not found");
        var runner = await _runners.GetByIdAsync(command.RunnerId, ct)
            ?? throw new InvalidOperationException($"Runner {command.RunnerId} not found");

        job.AssignTo(runner);
        runner.GoBusy();
        await _eventDispatcher.DispatchAsync(job.DomainEvents, ct);
        job.ClearDomainEvents();
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
