using Microsoft.EntityFrameworkCore;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Domain.ValueObjects;

namespace Orchestrator.Infrastructure.Persistence.Repositories;

public class EfBuildRepository(OrchestratorDbContext context) : IBuildRepository
{
    public async Task<Build?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await GetByIdAsync(id, includeJobs: true, ct);

    public async Task<Build?> GetByIdAsync(Guid id, bool includeJobs, CancellationToken ct = default)
    {
        var query = context.Builds.AsQueryable();
        if (includeJobs)
            query = query.Include(b => b.Jobs);
        return await query.FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<IReadOnlyCollection<Build>> GetByPipelineIdAsync(
        Guid pipelineId,
        CancellationToken ct = default
    ) => await context.Builds.Where(b => b.PipelineId == pipelineId).ToListAsync(ct);

    public async Task<Domain.Common.PagedResult<Build>> GetPagedByPipelineIdAsync(
        Guid pipelineId,
        int page,
        int pageSize,
        CancellationToken ct = default
    )
    {
        var query = context.Builds
            .Include(b => b.Jobs)
            .Where(b => b.PipelineId == pipelineId);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new Domain.Common.PagedResult<Build>(items, totalCount);
    }

    public async Task<IReadOnlyCollection<Build>> GetByStatusAsync(
        BuildStatus status,
        CancellationToken ct = default
    ) => await context.Builds.Where(b => b.Status == status).ToListAsync(ct);

    public async Task AddAsync(Build build, CancellationToken ct = default)
    {
        await context.Builds.AddAsync(build, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Build build, CancellationToken ct = default)
    {
        context.Builds.Update(build);
        await context.SaveChangesAsync(ct);
    }
}
