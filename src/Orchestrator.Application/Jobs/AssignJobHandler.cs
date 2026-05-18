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
        IDomainEventDispatcher eventDispatcher) { }

    public Task HandleAsync(AssignJobCommand command, CancellationToken ct = default)
        => throw new NotImplementedException();
}
