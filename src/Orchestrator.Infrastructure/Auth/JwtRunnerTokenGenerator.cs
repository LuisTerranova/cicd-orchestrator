using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Infrastructure.Auth;

public class JwtRunnerTokenGenerator : IRunnerTokenGenerator
{
    public string GenerateToken(Guid runnerId)
        => throw new NotImplementedException();

    public bool ValidateToken(string token, out Guid runnerId)
        => throw new NotImplementedException();
}
