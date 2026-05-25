using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orchestrator.Domain.Entities;

namespace Orchestrator.Infrastructure.Persistence.Configurations;

public class RunnerConfiguration : IEntityTypeConfiguration<Runner>
{
    public void Configure(EntityTypeBuilder<Runner> builder)
    {
        builder.ToTable("Runners");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(r => r.Labels).HasColumnType("text[]");
        builder.Property(r => r.Os).HasMaxLength(50);
        builder.Property(r => r.Arch).HasMaxLength(50);
        builder.Property(r => r.LastSeen).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.HasIndex(r => r.Name).IsUnique();
        builder.HasIndex(r => r.Status);
    }
}
