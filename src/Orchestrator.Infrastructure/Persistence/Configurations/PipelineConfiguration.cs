using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orchestrator.Domain.Entities;

namespace Orchestrator.Infrastructure.Persistence.Configurations;

public class PipelineConfiguration : IEntityTypeConfiguration<Pipeline>
{
    public void Configure(EntityTypeBuilder<Pipeline> builder)
    {
        builder.ToTable("Pipelines");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(255);
        builder.Property(p => p.Repo).IsRequired().HasMaxLength(500);
        builder.Property(p => p.Branch).HasMaxLength(255);
        builder.Property(p => p.YamlPath).HasMaxLength(500);
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.HasIndex(p => p.Name).IsUnique();
    }
}
