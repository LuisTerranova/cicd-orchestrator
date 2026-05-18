using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Pipelines;

public sealed class GetAllPipelinesQuery
{
    private readonly IPipelineRepository _pipelines;

    public GetAllPipelinesQuery(IPipelineRepository pipelines) { }

    public Task<IReadOnlyCollection<Domain.Entities.Pipeline>> HandleAsync(CancellationToken ct = default)
        => throw new NotImplementedException();
}
