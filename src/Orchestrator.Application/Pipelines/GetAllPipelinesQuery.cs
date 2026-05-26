using Orchestrator.Domain.Common;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Pipelines;

public sealed class GetAllPipelinesQuery(IPipelineRepository pipelines)
{
    public async Task<PagedResult<Domain.Entities.Pipeline>> HandleAsync(
        int page,
        int pageSize,
        CancellationToken ct = default
    ) => await pipelines.GetPagedAsync(page, pageSize, ct);
}
