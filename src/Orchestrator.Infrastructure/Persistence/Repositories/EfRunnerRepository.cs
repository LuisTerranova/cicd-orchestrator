using Microsoft.EntityFrameworkCore;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.ValueObjects;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Infrastructure.Persistence;

namespace Orchestrator.Infrastructure.Persistence.Repositories;

public class EfRunnerRepository : IRunnerRepository
{
    private readonly OrchestratorDbContext _context;

    public EfRunnerRepository(OrchestratorDbContext context) { }

    public Task<Runner?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<Runner?> GetByNameAsync(string name, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyCollection<Runner>> GetByStatusAsync(RunnerStatus status, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyCollection<Runner>> GetAllAsync(CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task AddAsync(Runner runner, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task UpdateAsync(Runner runner, CancellationToken ct = default)
        => throw new NotImplementedException();
}
