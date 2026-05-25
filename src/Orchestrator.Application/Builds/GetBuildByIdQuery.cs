using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Builds;

public sealed class GetBuildByIdQuery
{
    private readonly IBuildRepository _builds;

    public GetBuildByIdQuery(IBuildRepository builds)
    {
        _builds = builds;
    }

    public async Task<Domain.Entities.Build?> HandleAsync(Guid id, CancellationToken ct = default)
        => await _builds.GetByIdAsync(id, ct);
}
