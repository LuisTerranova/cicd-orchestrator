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
        IDomainEventDispatcher eventDispatcher)
    {
        _pipelines = pipelines;
        _unitOfWork = unitOfWork;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<Guid> HandleAsync(CreatePipelineCommand command, CancellationToken ct = default)
    {
        var pipeline = Domain.Entities.Pipeline.Create(command.Name, command.Repo, command.Branch, command.YamlPath);
        await _pipelines.AddAsync(pipeline, ct);
        await _eventDispatcher.DispatchAsync(pipeline.DomainEvents, ct);
        pipeline.ClearDomainEvents();
        await _unitOfWork.SaveChangesAsync(ct);
        return pipeline.Id;
    }
}
