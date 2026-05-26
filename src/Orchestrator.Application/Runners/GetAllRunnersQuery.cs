using Orchestrator.Domain.Common;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Runners;

public sealed class GetAllRunnersQuery(IRunnerRepository runners)
{
    public async Task<PagedResult<Domain.Entities.Runner>> HandleAsync(
        int page,
        int pageSize,
        CancellationToken ct = default
    ) => await runners.GetPagedAsync(page, pageSize, ct);
}
