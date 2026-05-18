using Orchestrator.Domain.Entities;

namespace Orchestrator.Domain.Interfaces;

public interface IBuildRepository
{
    Task<Build?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Build>> GetByPipelineIdAsync(Guid pipelineId, CancellationToken ct = default);
    Task<IReadOnlyCollection<Build>> GetByStatusAsync(ValueObjects.BuildStatus status, CancellationToken ct = default);
    Task AddAsync(Build build, CancellationToken ct = default);
    Task UpdateAsync(Build build, CancellationToken ct = default);
}
