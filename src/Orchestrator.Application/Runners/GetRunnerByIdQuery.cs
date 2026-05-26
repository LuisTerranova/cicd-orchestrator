using Orchestrator.Domain.Entities;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Runners;

public sealed class GetRunnerByIdQuery(IRunnerRepository runners)
{
    public async Task<Runner?> HandleAsync(Guid id, CancellationToken ct = default) =>
        await runners.GetByIdAsync(id, ct);
}
