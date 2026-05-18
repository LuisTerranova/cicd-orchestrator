namespace Orchestrator.Domain.Entities;

public class Artifact : Entity
{
    public Guid BuildId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string StoragePath { get; private set; } = string.Empty;
    public int SizeBytes { get; private set; }
    public string ContentType { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    private Artifact() { }

    public static Artifact Create(Guid buildId, string name, string storagePath, int sizeBytes, string contentType, DateTime? expiresAt = null)
        => throw new NotImplementedException();
}
