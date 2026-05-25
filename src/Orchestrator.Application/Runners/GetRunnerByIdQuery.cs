using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Runners;

public sealed class GetRunnerByIdQuery
{
    private readonly IRunnerRepository _runners;

    public GetRunnerByIdQuery(IRunnerRepository runners)
    {
        _runners = runners;
    }

    public async Task<Domain.Entities.Runner?> HandleAsync(Guid id, CancellationToken ct = default)
        => await _runners.GetByIdAsync(id, ct);
}
