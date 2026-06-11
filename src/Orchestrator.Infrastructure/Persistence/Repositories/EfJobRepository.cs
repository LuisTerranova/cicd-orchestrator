using Microsoft.EntityFrameworkCore;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Domain.ValueObjects;

namespace Orchestrator.Infrastructure.Persistence.Repositories;

public class EfJobRepository(OrchestratorDbContext context) : IJobRepository
{
    public async Task<Job?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Jobs.FirstOrDefaultAsync(j => j.Id == id, ct);

    public async Task<IReadOnlyCollection<Job>> GetByBuildIdAsync(
        Guid buildId,
        CancellationToken ct = default
    ) => await context.Jobs.Where(j => j.BuildId == buildId).ToListAsync(ct);

    public async Task<IReadOnlyCollection<Job>> GetByStatusAsync(
        JobStatus status,
        CancellationToken ct = default
    ) => await context.Jobs.Where(j => j.Status == status).ToListAsync(ct);

    public async Task AddAsync(Job job, CancellationToken ct = default)
    {
        await context.Jobs.AddAsync(job, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Job job, CancellationToken ct = default)
    {
        context.Jobs.Update(job);
        await context.SaveChangesAsync(ct);
    }
}
