using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Runners;

public sealed class GetAllRunnersQuery
{
    private readonly IRunnerRepository _runners;

    public GetAllRunnersQuery(IRunnerRepository runners)
    {
        _runners = runners;
    }

    public async Task<IReadOnlyCollection<Domain.Entities.Runner>> HandleAsync(CancellationToken ct = default)
        => await _runners.GetAllAsync(ct);
}
