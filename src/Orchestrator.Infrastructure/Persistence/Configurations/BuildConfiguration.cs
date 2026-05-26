using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orchestrator.Domain.Entities;

namespace Orchestrator.Infrastructure.Persistence.Configurations;

public class BuildConfiguration : IEntityTypeConfiguration<Build>
{
    public void Configure(EntityTypeBuilder<Build> builder)
    {
        builder.ToTable("Builds");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.TriggerEvent).IsRequired().HasMaxLength(100);
        builder.Property(b => b.CommitSha).HasMaxLength(40);
        builder.Property(b => b.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property(b => b.Priority).HasDefaultValue(0);
        builder
            .HasMany(b => b.Jobs)
            .WithOne()
            .HasForeignKey(j => j.BuildId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(b => b.PipelineId);
    }
}
