namespace Orchestrator.Runner.Container;

public sealed class PodmanCli
{
    private readonly Execution.ProcessInvoker _process;

    public PodmanCli(Execution.ProcessInvoker process)
    {
        _process = process;
    }

    public async Task PullAsync(string image, CancellationToken ct)
    {
        var result = await _process.RunAsync("podman", ["pull", image], ct);
        if (result.ExitCode != 0)
            throw new PodmanException($"Failed to pull image {image}: {result.Stderr}");
    }

    public async Task<string> CreateAsync(string image, ContainerSpec spec, CancellationToken ct)
    {
        var args = new List<string>
        {
            "create",
            "--name", spec.ContainerName,
            "--volume", $"{spec.WorkspacePath}:/workspace:rw",
            "--volume", $"{spec.SecretsPath}:/run/secrets:ro",
            "--network", spec.NetworkMode,
            "--memory", spec.MemoryLimit,
            "--cpus", spec.CpuLimit,
            "--read-only-rootfs",
            "--security-opt", "no-new-privileges:true",
            "--cap-drop", "ALL",
            "--label", $"orchestrator.job_id={spec.JobId}",
            image
        };

        if (spec.EnvVars is { Count: > 0 })
            foreach (var (key, value) in spec.EnvVars)
                args.AddRange(["--env", $"{key}={value}"]);

        var result = await _process.RunAsync("podman", [.. args], ct);
        return result.Stdout.Trim();
    }

    public async Task StartAsync(string containerId, CancellationToken ct)
    {
        await _process.RunAsync("podman", ["start", containerId], ct);
    }

    public async Task<int> ExecAsync(string containerId, string shell, string scriptPath, CancellationToken ct)
    {
        var result = await _process.RunAsync("podman",
            ["exec", "-i", containerId, shell, scriptPath], ct);
        return result.ExitCode;
    }

    public async Task StopAsync(string containerId, TimeSpan gracePeriod, CancellationToken ct)
    {
        await _process.RunAsync("podman",
            ["stop", "--time", ((int)gracePeriod.TotalSeconds).ToString(), containerId], ct);
    }

    public async Task RemoveAsync(string containerId, CancellationToken ct)
    {
        await _process.RunAsync("podman", ["rm", "--force", containerId], ct);
    }

    public async Task<List<string>> ListOrphanedAsync(Guid[] activeJobIds, CancellationToken ct)
    {
        var result = await _process.RunAsync("podman",
            ["ps", "-a", "--filter", "label=orchestrator.job_id", "--format", "{{.ID}} {{.Label \"orchestrator.job_id\"}}"], ct);

        return result.Stdout
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Split(' ', 2))
            .Where(parts => parts.Length == 2 && !activeJobIds.Contains(Guid.Parse(parts[1])))
            .Select(parts => parts[0])
            .ToList();
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
