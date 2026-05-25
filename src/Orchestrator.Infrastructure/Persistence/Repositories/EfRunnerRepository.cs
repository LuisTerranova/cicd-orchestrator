using Microsoft.EntityFrameworkCore;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.ValueObjects;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Infrastructure.Persistence.Repositories;

public class EfRunnerRepository : IRunnerRepository
{
    private readonly OrchestratorDbContext _context;

    public EfRunnerRepository(OrchestratorDbContext context)
    {
        _context = context;
    }

    public async Task<Runner?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Runners.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<Runner?> GetByNameAsync(string name, CancellationToken ct = default)
        => await _context.Runners.FirstOrDefaultAsync(r => r.Name == name, ct);

    public async Task<IReadOnlyCollection<Runner>> GetByStatusAsync(RunnerStatus status, CancellationToken ct = default)
        => await _context.Runners.Where(r => r.Status == status).ToListAsync(ct);

    public async Task<IReadOnlyCollection<Runner>> GetAllAsync(CancellationToken ct = default)
        => await _context.Runners.ToListAsync(ct);

    public async Task AddAsync(Runner runner, CancellationToken ct = default)
        => await _context.Runners.AddAsync(runner, ct);

    public Task UpdateAsync(Runner runner, CancellationToken ct = default)
    {
        _context.Runners.Update(runner);
        return Task.CompletedTask;
    }
}
