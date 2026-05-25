using Microsoft.EntityFrameworkCore;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.ValueObjects;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Infrastructure.Persistence.Repositories;

public class EfJobRepository : IJobRepository
{
    private readonly OrchestratorDbContext _context;

    public EfJobRepository(OrchestratorDbContext context)
    {
        _context = context;
    }

    public async Task<Job?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id, ct);

    public async Task<IReadOnlyCollection<Job>> GetByBuildIdAsync(Guid buildId, CancellationToken ct = default)
        => await _context.Jobs.Where(j => j.BuildId == buildId).ToListAsync(ct);

    public async Task<IReadOnlyCollection<Job>> GetByStatusAsync(JobStatus status, CancellationToken ct = default)
        => await _context.Jobs.Where(j => j.Status == status).ToListAsync(ct);

    public async Task AddAsync(Job job, CancellationToken ct = default)
        => await _context.Jobs.AddAsync(job, ct);

    public Task UpdateAsync(Job job, CancellationToken ct = default)
    {
        _context.Jobs.Update(job);
        return Task.CompletedTask;
    }
}
