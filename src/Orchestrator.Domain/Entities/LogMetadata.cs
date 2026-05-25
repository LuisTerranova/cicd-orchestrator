using Orchestrator.Domain.Exceptions;

namespace Orchestrator.Domain.Entities;

public class LogMetadata : Entity
{
    public Guid JobId { get; private set; }
    public string FilePath { get; private set; } = string.Empty;
    public int LineCount { get; private set; }
    public long SizeBytes { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private LogMetadata() { }

    public static LogMetadata Create(Guid jobId, string filePath, int lineCount, long sizeBytes)
    {
        if (jobId == Guid.Empty)
            throw new DomainException("JobId cannot be empty.");
        if (string.IsNullOrWhiteSpace(filePath))
            throw new DomainException("FilePath cannot be empty.");
        if (lineCount < 0)
            throw new DomainException("LineCount cannot be negative.");
        if (sizeBytes < 0)
            throw new DomainException("SizeBytes cannot be negative.");

        return new LogMetadata
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            FilePath = filePath,
            LineCount = lineCount,
            SizeBytes = sizeBytes,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(int lineCount, long sizeBytes)
    {
        if (lineCount < 0)
            throw new DomainException("LineCount cannot be negative.");
        if (sizeBytes < 0)
            throw new DomainException("SizeBytes cannot be negative.");

        LineCount = lineCount;
        SizeBytes = sizeBytes;
    }
}

