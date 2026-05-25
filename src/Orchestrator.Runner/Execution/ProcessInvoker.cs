using System.Diagnostics;

namespace Orchestrator.Runner.Execution;

public sealed class ProcessInvoker
{
    public async Task<ProcessResult> RunAsync(string command, string[] args, CancellationToken ct)
    {
        var psi = new ProcessStartInfo
        {
            FileName = command,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var arg in args)
            psi.ArgumentList.Add(arg);

        using var process = new Process { StartInfo = psi };
        process.Start();

        using var _ = ct.Register(() => KillTree(process.Id));

        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync(ct);

        return new ProcessResult(process.ExitCode, await stdout, await stderr);
    }

    public void KillTree(int pid)
    {
        try
        {
            using var proc = Process.GetProcessById(pid);
            proc.Kill(entireProcessTree: true);
        }
        catch (ArgumentException)
        {
            // Process already exited — nothing to kill
        }
    }
}

public sealed record ProcessResult(
    int ExitCode,
    string Stdout,
    string Stderr
);
