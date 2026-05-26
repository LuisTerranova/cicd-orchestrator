using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Builds;

public sealed class GetBuildsByPipelineIdQuery(IBuildRepository builds)
{
    public async Task<IReadOnlyCollection<Domain.Entities.Build>> HandleAsync(
        Guid pipelineId,
        CancellationToken ct = default
    ) => await builds.GetByPipelineIdAsync(pipelineId, ct);
}
