using Microsoft.EntityFrameworkCore;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.ValueObjects;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Infrastructure.Persistence;

namespace Orchestrator.Infrastructure.Persistence.Repositories;

public class EfJobRepository : IJobRepository
{
    private readonly OrchestratorDbContext _context;

    public EfJobRepository(OrchestratorDbContext context) { }

    public Task<Job?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyCollection<Job>> GetByBuildIdAsync(Guid buildId, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyCollection<Job>> GetByStatusAsync(JobStatus status, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task AddAsync(Job job, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task UpdateAsync(Job job, CancellationToken ct = default)
        => throw new NotImplementedException();
}
