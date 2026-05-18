using Microsoft.EntityFrameworkCore;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.ValueObjects;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Infrastructure.Persistence;

namespace Orchestrator.Infrastructure.Persistence.Repositories;

public class EfBuildRepository : IBuildRepository
{
    private readonly OrchestratorDbContext _context;

    public EfBuildRepository(OrchestratorDbContext context) { }

    public Task<Build?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyCollection<Build>> GetByPipelineIdAsync(Guid pipelineId, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyCollection<Build>> GetByStatusAsync(BuildStatus status, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task AddAsync(Build build, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task UpdateAsync(Build build, CancellationToken ct = default)
        => throw new NotImplementedException();
}
