using Microsoft.EntityFrameworkCore;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Infrastructure.Persistence;

namespace Orchestrator.Infrastructure.Persistence.Repositories;

public class EfArtifactRepository : IArtifactRepository
{
    private readonly OrchestratorDbContext _context;

    public EfArtifactRepository(OrchestratorDbContext context) { }

    public Task<Artifact?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyCollection<Artifact>> GetByBuildIdAsync(Guid buildId, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task AddAsync(Artifact artifact, CancellationToken ct = default)
        => throw new NotImplementedException();
}
