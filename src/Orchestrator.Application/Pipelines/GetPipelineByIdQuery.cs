using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Pipelines;

public sealed class GetPipelineByIdQuery
{
    private readonly IPipelineRepository _pipelines;

    public GetPipelineByIdQuery(IPipelineRepository pipelines) { }

    public Task<Domain.Entities.Pipeline?> HandleAsync(Guid id, CancellationToken ct = default)
        => throw new NotImplementedException();
}
