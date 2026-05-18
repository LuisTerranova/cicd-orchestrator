using Orchestrator.Domain.Entities;

namespace Orchestrator.Domain.Interfaces;

public interface IRunnerRepository
{
    Task<Runner?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Runner?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyCollection<Runner>> GetByStatusAsync(ValueObjects.RunnerStatus status, CancellationToken ct = default);
    Task<IReadOnlyCollection<Runner>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Runner runner, CancellationToken ct = default);
    Task UpdateAsync(Runner runner, CancellationToken ct = default);
}
