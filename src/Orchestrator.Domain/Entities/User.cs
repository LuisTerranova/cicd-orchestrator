using Orchestrator.Domain;

namespace Orchestrator.Domain.Entities;

public class User : Entity
{
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = "Admin";
    public DateTime CreatedAt { get; private set; }

    private User() { }

    public static User Create(string username, string passwordHash, string role = "Admin")
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.");

        return new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = passwordHash,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
    }
}
