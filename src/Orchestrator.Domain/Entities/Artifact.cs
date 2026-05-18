using Orchestrator.Domain.Exceptions;

namespace Orchestrator.Domain.Entities;

public class Artifact : Entity
{
    public Guid BuildId { get; private set; }
    public required string Name { get; private set; }
    public required string Path { get; private set; }
    public int SizeBytes { get; private set; }
    public string ContentType { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    private Artifact() { }

    public static Artifact Create(Guid buildId, string name, string path, int sizeBytes, string contentType, DateTime? expiresAt = null) { }
}
