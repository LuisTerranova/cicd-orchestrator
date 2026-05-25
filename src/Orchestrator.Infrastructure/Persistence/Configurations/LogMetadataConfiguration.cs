using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orchestrator.Domain.Entities;

namespace Orchestrator.Infrastructure.Persistence.Configurations;

public class LogMetadataConfiguration : IEntityTypeConfiguration<LogMetadata>
{
    public void Configure(EntityTypeBuilder<LogMetadata> builder)
    {
        builder.ToTable("LogMetadata");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.FilePath).IsRequired().HasMaxLength(1024);
        builder.Property(l => l.CreatedAt).IsRequired();
        builder.HasOne<Job>().WithOne().HasForeignKey<LogMetadata>(l => l.JobId).OnDelete(DeleteBehavior.Cascade);
    }
}
