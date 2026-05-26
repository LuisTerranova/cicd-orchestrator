using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Builds;

public sealed class GetBuildByIdQuery(IBuildRepository builds)
{
    public async Task<Domain.Entities.Build?> HandleAsync(
        Guid id,
        CancellationToken ct = default
    ) => await builds.GetByIdAsync(id, ct);
}
