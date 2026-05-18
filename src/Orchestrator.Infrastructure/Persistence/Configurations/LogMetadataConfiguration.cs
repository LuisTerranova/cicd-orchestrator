using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orchestrator.Domain.Entities;

namespace Orchestrator.Infrastructure.Persistence.Configurations;

public class LogMetadataConfiguration : IEntityTypeConfiguration<LogMetadata>
{
    public void Configure(EntityTypeBuilder<LogMetadata> builder) { }
}
