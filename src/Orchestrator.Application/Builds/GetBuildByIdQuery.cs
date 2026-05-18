using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Builds;

public sealed class GetBuildByIdQuery
{
    private readonly IBuildRepository _builds;

    public GetBuildByIdQuery(IBuildRepository builds) { }

    public Task<Domain.Entities.Build?> HandleAsync(Guid id, CancellationToken ct = default)
        => throw new NotImplementedException();
}
