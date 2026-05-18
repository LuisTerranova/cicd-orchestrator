namespace Orchestrator.Domain.Interfaces;

public interface IRunnerTokenGenerator
{
    string GenerateToken(Guid runnerId);
    bool ValidateToken(string token, out Guid runnerId);
}
