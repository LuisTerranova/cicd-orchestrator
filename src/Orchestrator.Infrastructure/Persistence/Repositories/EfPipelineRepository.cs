using Microsoft.EntityFrameworkCore;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Infrastructure.Persistence;

namespace Orchestrator.Infrastructure.Persistence.Repositories;

public class EfPipelineRepository : IPipelineRepository
{
    private readonly OrchestratorDbContext _context;

    public EfPipelineRepository(OrchestratorDbContext context) { }

    public Task<Pipeline?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyCollection<Pipeline>> GetAllAsync(CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task AddAsync(Pipeline pipeline, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task UpdateAsync(Pipeline pipeline, CancellationToken ct = default)
        => throw new NotImplementedException();
}
