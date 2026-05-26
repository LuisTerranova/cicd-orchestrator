using Orchestrator.Application.Common;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Builds;

public sealed class TriggerBuildHandler(
    IPipelineRepository pipelines,
    IBuildRepository builds,
    IDomainEventDispatcher eventDispatcher
) : ICommandHandler<TriggerBuildCommand, Guid>
{
    public async Task<Guid> HandleAsync(TriggerBuildCommand command, CancellationToken ct = default)
    {
        var pipeline =
            await pipelines.GetByIdAsync(command.PipelineId, ct)
            ?? throw new InvalidOperationException($"Pipeline {command.PipelineId} not found");

        var build = pipeline.TriggerBuild(
            command.TriggerEvent,
            command.CommitSha,
            command.Priority
        );
        await builds.AddAsync(build, ct);
        await eventDispatcher.DispatchAsync(build.DomainEvents, ct);
        build.ClearDomainEvents();
        return build.Id;
    }
}
