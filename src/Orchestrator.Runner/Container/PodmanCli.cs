namespace Orchestrator.Runner.Container;

public sealed class PodmanCli
{
    public PodmanCli(Execution.ProcessInvoker process)
    {
        throw new NotImplementedException();
    }

    public async Task PullAsync(string image, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<string> CreateAsync(string image, ContainerSpec spec, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task StartAsync(string containerId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<int> ExecAsync(string containerId, string shell, string scriptPath,
        IObservable<string> stdout, IObservable<string> stderr, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task StopAsync(string containerId, TimeSpan gracePeriod, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task RemoveAsync(string containerId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<List<string>> ListOrphanedAsync(Guid[] activeJobIds, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}

public sealed record ContainerSpec(
    Guid JobId,
    string ContainerName,
    string Image,
    string WorkspacePath,
    string SecretsPath,
    string NetworkMode = "none",
    string MemoryLimit = "2g",
    string CpuLimit = "2.0",
    Dictionary<string, string>? EnvVars = null
);

public sealed class PodmanException : Exception
{
    public PodmanException(string message) : base(message) { }
}
