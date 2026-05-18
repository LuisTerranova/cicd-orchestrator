using Orchestrator.Application.Common;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Pipelines;

public sealed class CreatePipelineHandler
{
    private readonly IPipelineRepository _pipelines;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public CreatePipelineHandler(
        IPipelineRepository pipelines,
        IUnitOfWork unitOfWork,
        IDomainEventDispatcher eventDispatcher) { }

    public Task<Guid> HandleAsync(CreatePipelineCommand command, CancellationToken ct = default)
        => throw new NotImplementedException();
}
