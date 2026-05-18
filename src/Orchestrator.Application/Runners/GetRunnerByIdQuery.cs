using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Runners;

public sealed class GetRunnerByIdQuery
{
    private readonly IRunnerRepository _runners;

    public GetRunnerByIdQuery(IRunnerRepository runners) { }

    public Task<Domain.Entities.Runner?> HandleAsync(Guid id, CancellationToken ct = default)
        => throw new NotImplementedException();
}
