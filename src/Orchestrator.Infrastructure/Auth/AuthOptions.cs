namespace Orchestrator.Infrastructure.Auth;

public sealed record AuthOptions
{
    public string SecretKey { get; init; } = string.Empty;
    public int TokenExpirationDays { get; init; } = 30;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SecretKey) || SecretKey.Length < 32)
            throw new InvalidOperationException(
                "Auth:SecretKey must be at least 32 characters long.");
    }
}
