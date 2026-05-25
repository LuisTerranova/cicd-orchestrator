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
        IDomainEventDispatcher eventDispatcher)
    {
        _pipelines = pipelines;
        _builds = builds;
        _unitOfWork = unitOfWork;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<Guid> HandleAsync(TriggerBuildCommand command, CancellationToken ct = default)
    {
        var pipeline = await _pipelines.GetByIdAsync(command.PipelineId, ct)
            ?? throw new InvalidOperationException($"Pipeline {command.PipelineId} not found");

        var build = pipeline.TriggerBuild(command.TriggerEvent, command.CommitSha, command.Priority);
        await _builds.AddAsync(build, ct);
        await _eventDispatcher.DispatchAsync(build.DomainEvents, ct);
        build.ClearDomainEvents();
        await _unitOfWork.SaveChangesAsync(ct);
        return build.Id;
    }
}
