using Microsoft.EntityFrameworkCore;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Infrastructure.Persistence.Repositories;

public class EfPipelineRepository(OrchestratorDbContext context) : IPipelineRepository
{
    public async Task<Pipeline?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Pipelines.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyCollection<Pipeline>> GetAllAsync(CancellationToken ct = default) =>
        await context.Pipelines.ToListAsync(ct);

    public async Task<Domain.Common.PagedResult<Pipeline>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken ct = default
    )
    {
        var query = context.Pipelines.AsNoTracking();
        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new Domain.Common.PagedResult<Pipeline>(items, totalCount);
    }

    public async Task AddAsync(Pipeline pipeline, CancellationToken ct = default) =>
        await context.Pipelines.AddAsync(pipeline, ct);

    public Task UpdateAsync(Pipeline pipeline, CancellationToken ct = default)
    {
        context.Pipelines.Update(pipeline);
        return Task.CompletedTask;
    }
}
