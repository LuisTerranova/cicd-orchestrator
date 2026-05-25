using Microsoft.EntityFrameworkCore;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Infrastructure.Persistence.Repositories;

public class EfPipelineRepository : IPipelineRepository
{
    private readonly OrchestratorDbContext _context;

    public EfPipelineRepository(OrchestratorDbContext context)
    {
        _context = context;
    }

    public async Task<Pipeline?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Pipelines.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyCollection<Pipeline>> GetAllAsync(CancellationToken ct = default)
        => await _context.Pipelines.ToListAsync(ct);

    public async Task AddAsync(Pipeline pipeline, CancellationToken ct = default)
        => await _context.Pipelines.AddAsync(pipeline, ct);

    public Task UpdateAsync(Pipeline pipeline, CancellationToken ct = default)
    {
        _context.Pipelines.Update(pipeline);
        return Task.CompletedTask;
    }
}
