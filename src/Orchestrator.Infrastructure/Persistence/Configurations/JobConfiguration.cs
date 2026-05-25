using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orchestrator.Domain.Entities;

namespace Orchestrator.Infrastructure.Persistence.Configurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("Jobs");
        builder.HasKey(j => j.Id);
        builder.Property(j => j.StageName).IsRequired().HasMaxLength(200);
        builder.Property(j => j.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(j => j.Version).HasDefaultValue(1);
        builder.HasIndex(j => j.BuildId);
        builder.HasIndex(j => j.RunnerId);
        builder.HasIndex(j => j.Status);
    }
}
