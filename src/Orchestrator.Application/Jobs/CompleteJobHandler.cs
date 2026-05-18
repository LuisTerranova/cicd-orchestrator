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
        IDomainEventDispatcher eventDispatcher) { }

    public Task HandleAsync(CompleteJobCommand command, CancellationToken ct = default)
        => throw new NotImplementedException();
}
