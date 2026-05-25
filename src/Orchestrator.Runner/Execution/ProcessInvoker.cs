namespace Orchestrator.Runner.Execution;

public sealed class ProcessInvoker
{
    public async Task<ProcessResult> RunAsync(string command, string[] args, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public void KillTree(int pid)
    {
        throw new NotImplementedException();
    }
}

public sealed record ProcessResult(
    int ExitCode,
    string Stdout,
    string Stderr
);
