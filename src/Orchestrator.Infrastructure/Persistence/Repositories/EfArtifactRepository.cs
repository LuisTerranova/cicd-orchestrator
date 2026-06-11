using Microsoft.EntityFrameworkCore;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Infrastructure.Persistence.Repositories;

public class EfArtifactRepository(OrchestratorDbContext context) : IArtifactRepository
{
    public async Task<Artifact?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Artifacts.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyCollection<Artifact>> GetByBuildIdAsync(
        Guid buildId,
        CancellationToken ct = default
    ) => await context.Artifacts.Where(a => a.BuildId == buildId).ToListAsync(ct);

    public async Task AddAsync(Artifact artifact, CancellationToken ct = default)
    {
        await context.Artifacts.AddAsync(artifact, ct);
        await context.SaveChangesAsync(ct);
    }
}
