using Orchestrator.Domain.Exceptions;

namespace Orchestrator.Domain.Entities;

public class LogMetadata : Entity
{
    public Guid JobId { get; private set; }
    public required string FilePath { get; private set; }
    public int LineCount { get; private set; }
    public long SizeBytes { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private LogMetadata() { }

    public static LogMetadata Create(Guid jobId, string filePath, int lineCount, long sizeBytes) { }
}
