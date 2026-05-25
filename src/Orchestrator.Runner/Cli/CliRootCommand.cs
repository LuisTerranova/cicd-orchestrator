namespace Orchestrator.Runner.Cli;

public sealed class CliRootCommand
{
    public CliParseResult Invoke(string[] args)
    {
        throw new NotImplementedException();
    }
}

public sealed record CliParseResult
{
    public T GetValue<T>(string name)
    {
        throw new NotImplementedException();
    }
}
