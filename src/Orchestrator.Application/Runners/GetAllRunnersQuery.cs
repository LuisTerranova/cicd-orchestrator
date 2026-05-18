using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Runners;

public sealed class GetAllRunnersQuery
{
    private readonly IRunnerRepository _runners;

    public GetAllRunnersQuery(IRunnerRepository runners) { }

    public Task<IReadOnlyCollection<Domain.Entities.Runner>> HandleAsync(CancellationToken ct = default)
        => throw new NotImplementedException();
}
