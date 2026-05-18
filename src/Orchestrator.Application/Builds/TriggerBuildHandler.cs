using Orchestrator.Application.Common;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Builds;

public sealed class TriggerBuildHandler
{
    private readonly IPipelineRepository _pipelines;
    private readonly IBuildRepository _builds;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public TriggerBuildHandler(
        IPipelineRepository pipelines,
        IBuildRepository builds,
        IUnitOfWork unitOfWork,
        IDomainEventDispatcher eventDispatcher) { }

    public Task<Guid> HandleAsync(TriggerBuildCommand command, CancellationToken ct = default)
        => throw new NotImplementedException();
}
