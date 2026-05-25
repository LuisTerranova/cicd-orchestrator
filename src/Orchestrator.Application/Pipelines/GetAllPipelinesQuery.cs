using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Pipelines;

public sealed class GetAllPipelinesQuery
{
    private readonly IPipelineRepository _pipelines;

    public GetAllPipelinesQuery(IPipelineRepository pipelines)
    {
        _pipelines = pipelines;
    }

    public async Task<IReadOnlyCollection<Domain.Entities.Pipeline>> HandleAsync(CancellationToken ct = default)
        => await _pipelines.GetAllAsync(ct);
}
