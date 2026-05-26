using Microsoft.EntityFrameworkCore;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Infrastructure.Persistence.Repositories;

public class EfLogRepository(OrchestratorDbContext context) : ILogRepository
{
    public async Task<LogMetadata?> GetByJobIdAsync(Guid jobId, CancellationToken ct = default) =>
        await context.Logs.FirstOrDefaultAsync(l => l.JobId == jobId, ct);

    public async Task AddAsync(LogMetadata log, CancellationToken ct = default) =>
        await context.Logs.AddAsync(log, ct);

    public Task UpdateAsync(LogMetadata log, CancellationToken ct = default)
    {
        context.Logs.Update(log);
        return Task.CompletedTask;
    }
}
