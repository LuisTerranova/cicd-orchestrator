using Orchestrator.Application.Common;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Pipelines;

public sealed class CreatePipelineHandler(
    IPipelineRepository pipelines,
    IDomainEventDispatcher eventDispatcher
) : ICommandHandler<CreatePipelineCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        CreatePipelineCommand command,
        CancellationToken ct = default
    )
    {
        var pipeline = Domain.Entities.Pipeline.Create(
            command.Name,
            command.Repo,
            command.Branch,
            command.YamlPath
        );
        await pipelines.AddAsync(pipeline, ct);
        await eventDispatcher.DispatchAsync(pipeline.DomainEvents, ct);
        pipeline.ClearDomainEvents();
        return pipeline.Id;
    }
}
