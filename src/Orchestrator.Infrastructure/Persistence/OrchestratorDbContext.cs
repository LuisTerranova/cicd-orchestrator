using Microsoft.EntityFrameworkCore;
using Orchestrator.Domain.Entities;

namespace Orchestrator.Infrastructure.Persistence;

public class OrchestratorDbContext : DbContext
{
    public OrchestratorDbContext(DbContextOptions<OrchestratorDbContext> options)
        : base(options) { }

    public DbSet<Pipeline> Pipelines => Set<Pipeline>();
    public DbSet<Build> Builds => Set<Build>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Runner> Runners => Set<Runner>();
    public DbSet<LogMetadata> Logs => Set<LogMetadata>();
    public DbSet<Artifact> Artifacts => Set<Artifact>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrchestratorDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        base.SaveChangesAsync(ct);
}
