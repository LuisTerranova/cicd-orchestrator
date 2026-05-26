using Orchestrator.Domain.Entities;

namespace Orchestrator.Domain.Interfaces;

public interface IPipelineRepository
{
    Task<Pipeline?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Pipeline>> GetAllAsync(CancellationToken ct = default);
    Task<Common.PagedResult<Pipeline>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken ct = default
    );
    Task AddAsync(Pipeline pipeline, CancellationToken ct = default);
    Task UpdateAsync(Pipeline pipeline, CancellationToken ct = default);
}
