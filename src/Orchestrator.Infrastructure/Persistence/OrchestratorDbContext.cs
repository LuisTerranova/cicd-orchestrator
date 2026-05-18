using Microsoft.EntityFrameworkCore;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Infrastructure.Persistence;

public class OrchestratorDbContext : DbContext, IUnitOfWork
{
    public OrchestratorDbContext(DbContextOptions<OrchestratorDbContext> options) : base(options) { }

    public DbSet<Pipeline> Pipelines => Set<Pipeline>();
    public DbSet<Build> Builds => Set<Build>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Runner> Runners => Set<Runner>();
    public DbSet<LogMetadata> Logs => Set<LogMetadata>();
    public DbSet<Artifact> Artifacts => Set<Artifact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) { }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        => throw new NotImplementedException();
}
