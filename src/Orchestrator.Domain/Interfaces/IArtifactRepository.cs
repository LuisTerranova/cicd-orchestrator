using Orchestrator.Domain.Entities;

namespace Orchestrator.Domain.Interfaces;

public interface IArtifactRepository
{
    Task<Artifact?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Artifact>> GetByBuildIdAsync(Guid buildId, CancellationToken ct = default);
    Task AddAsync(Artifact artifact, CancellationToken ct = default);
}
