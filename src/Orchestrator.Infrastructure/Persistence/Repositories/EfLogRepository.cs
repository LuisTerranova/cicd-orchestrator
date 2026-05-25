using Microsoft.EntityFrameworkCore;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Infrastructure.Persistence.Repositories;

public class EfLogRepository : ILogRepository
{
    private readonly OrchestratorDbContext _context;

    public EfLogRepository(OrchestratorDbContext context)
    {
        _context = context;
    }

    public async Task<LogMetadata?> GetByJobIdAsync(Guid jobId, CancellationToken ct = default)
        => await _context.Logs.FirstOrDefaultAsync(l => l.JobId == jobId, ct);

    public async Task AddAsync(LogMetadata log, CancellationToken ct = default)
        => await _context.Logs.AddAsync(log, ct);

    public Task UpdateAsync(LogMetadata log, CancellationToken ct = default)
    {
        _context.Logs.Update(log);
        return Task.CompletedTask;
    }
}
