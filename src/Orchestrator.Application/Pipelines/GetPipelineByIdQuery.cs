using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Pipelines;

public sealed class GetPipelineByIdQuery(IPipelineRepository pipelines)
{
    public async Task<Domain.Entities.Pipeline?> HandleAsync(
        Guid id,
        CancellationToken ct = default
    ) => await pipelines.GetByIdAsync(id, ct);
}
