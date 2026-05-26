using Orchestrator.Domain.Entities;

namespace Orchestrator.Domain.Interfaces;

public interface IJobRepository
{
    Task<Job?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Job>> GetByBuildIdAsync(Guid buildId, CancellationToken ct = default);
    Task<IReadOnlyCollection<Job>> GetByStatusAsync(
        ValueObjects.JobStatus status,
        CancellationToken ct = default
    );
    Task AddAsync(Job job, CancellationToken ct = default);
    Task UpdateAsync(Job job, CancellationToken ct = default);
}
