using Microsoft.EntityFrameworkCore;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.ValueObjects;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Infrastructure.Persistence.Repositories;

public class EfBuildRepository : IBuildRepository
{
    private readonly OrchestratorDbContext _context;

    public EfBuildRepository(OrchestratorDbContext context)
    {
        _context = context;
    }

    public async Task<Build?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Builds.Include(b => b.Jobs).FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<IReadOnlyCollection<Build>> GetByPipelineIdAsync(Guid pipelineId, CancellationToken ct = default)
        => await _context.Builds.Where(b => b.PipelineId == pipelineId).ToListAsync(ct);

    public async Task<IReadOnlyCollection<Build>> GetByStatusAsync(BuildStatus status, CancellationToken ct = default)
        => await _context.Builds.Where(b => b.Status == status).ToListAsync(ct);

    public async Task AddAsync(Build build, CancellationToken ct = default)
        => await _context.Builds.AddAsync(build, ct);

    public Task UpdateAsync(Build build, CancellationToken ct = default)
    {
        _context.Builds.Update(build);
        return Task.CompletedTask;
    }
}
