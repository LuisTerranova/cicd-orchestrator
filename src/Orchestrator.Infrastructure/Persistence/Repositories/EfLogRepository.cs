using Microsoft.EntityFrameworkCore;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Infrastructure.Persistence;

namespace Orchestrator.Infrastructure.Persistence.Repositories;

public class EfLogRepository : ILogRepository
{
    private readonly OrchestratorDbContext _context;

    public EfLogRepository(OrchestratorDbContext context) { }

    public Task<LogMetadata?> GetByJobIdAsync(Guid jobId, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task AddAsync(LogMetadata log, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task UpdateAsync(LogMetadata log, CancellationToken ct = default)
        => throw new NotImplementedException();
}
