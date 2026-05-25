using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Builds;

public sealed class GetBuildsByPipelineIdQuery
{
    private readonly IBuildRepository _builds;

    public GetBuildsByPipelineIdQuery(IBuildRepository builds)
    {
        _builds = builds;
    }

    public async Task<IReadOnlyCollection<Domain.Entities.Build>> HandleAsync(Guid pipelineId, CancellationToken ct = default)
        => await _builds.GetByPipelineIdAsync(pipelineId, ct);
}
