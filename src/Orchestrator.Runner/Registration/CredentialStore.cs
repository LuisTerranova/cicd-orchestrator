namespace Orchestrator.Runner.Registration;

public sealed class CredentialStore
{
    public async Task<(string RunnerId, string Secret)?> LoadAsync()
    {
        throw new NotImplementedException();
    }

    public async Task SaveAsync(string runnerId, string secret)
    {
        throw new NotImplementedException();
    }

    public bool Exists()
    {
        throw new NotImplementedException();
    }
}
