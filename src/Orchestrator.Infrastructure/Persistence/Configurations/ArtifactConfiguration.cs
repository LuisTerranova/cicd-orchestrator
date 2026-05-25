using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orchestrator.Domain.Entities;

namespace Orchestrator.Infrastructure.Persistence.Configurations;

public class ArtifactConfiguration : IEntityTypeConfiguration<Artifact>
{
    public void Configure(EntityTypeBuilder<Artifact> builder)
    {
        builder.ToTable("Artifacts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(255);
        builder.Property(a => a.StoragePath).IsRequired().HasMaxLength(1024);
        builder.Property(a => a.ContentType).HasMaxLength(100);
        builder.Property(a => a.SizeBytes).IsRequired();
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.HasIndex(a => a.BuildId);
    }
}
