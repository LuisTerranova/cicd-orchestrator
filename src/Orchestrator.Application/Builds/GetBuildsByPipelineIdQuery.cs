using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Builds;

public sealed class GetBuildsByPipelineIdQuery
{
    private readonly IBuildRepository _builds;

    public GetBuildsByPipelineIdQuery(IBuildRepository builds) { }

    public Task<IReadOnlyCollection<Domain.Entities.Build>> HandleAsync(Guid pipelineId, CancellationToken ct = default)
        => throw new NotImplementedException();
}
