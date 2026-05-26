using Microsoft.EntityFrameworkCore;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Domain.ValueObjects;

namespace Orchestrator.Infrastructure.Persistence.Repositories;

public class EfRunnerRepository(OrchestratorDbContext context) : IRunnerRepository
{
    public async Task<Runner?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Runners.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<Runner?> GetByNameAsync(string name, CancellationToken ct = default) =>
        await context.Runners.FirstOrDefaultAsync(r => r.Name == name, ct);

    public async Task<IReadOnlyCollection<Runner>> GetByStatusAsync(
        RunnerStatus status,
        CancellationToken ct = default
    ) => await context.Runners.Where(r => r.Status == status).ToListAsync(ct);

    public async Task<IReadOnlyCollection<Runner>> GetAllAsync(CancellationToken ct = default) =>
        await context.Runners.ToListAsync(ct);

    public async Task<Domain.Common.PagedResult<Runner>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken ct = default
    )
    {
        var query = context.Runners.AsNoTracking();
        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new Domain.Common.PagedResult<Runner>(items, totalCount);
    }

    public async Task AddAsync(Runner runner, CancellationToken ct = default) =>
        await context.Runners.AddAsync(runner, ct);

    public Task UpdateAsync(Runner runner, CancellationToken ct = default)
    {
        context.Runners.Update(runner);
        return Task.CompletedTask;
    }
}
