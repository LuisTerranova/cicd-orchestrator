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
            CreateNoWindow = true,
        };

        foreach (var arg in args)
            psi.ArgumentList.Add(arg);

        using var process = new Process { StartInfo = psi };
        process.Start();

        using var reg = ct.Register(() =>
        {
            try { KillTree(process.Id); }
            catch { /* best effort */ }
        });

        var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
        var stderrTask = process.StandardError.ReadToEndAsync(ct);

        await process.WaitForExitAsync(ct);

        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        return new ProcessResult(process.ExitCode, stdout, stderr);
    }

    public async Task<ProcessResult> RunWithLogCaptureAsync(
        string command, string[] args,
        Action<string, string>? onLine,
        CancellationToken ct)
    {
        var psi = new ProcessStartInfo
        {
            FileName = command,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        foreach (var arg in args)
            psi.ArgumentList.Add(arg);

        using var process = new Process { StartInfo = psi };
        process.Start();

        using var reg = ct.Register(() =>
        {
            try { KillTree(process.Id); }
            catch { /* best effort */ }
        });

        var stdoutLines = new List<string>();
        var stderrLines = new List<string>();

        var stdoutTask = ReadLinesAsync(process.StandardOutput, "stdout", onLine, stdoutLines, ct);
        var stderrTask = ReadLinesAsync(process.StandardError, "stderr", onLine, stderrLines, ct);

        await Task.WhenAll(process.WaitForExitAsync(ct), stdoutTask, stderrTask);

        return new ProcessResult(process.ExitCode, string.Join("\n", stdoutLines), string.Join("\n", stderrLines));
    }

    private static async Task ReadLinesAsync(
        StreamReader reader, string streamName,
        Action<string, string>? onLine,
        List<string> lines,
        CancellationToken ct)
    {
        try
        {
            while (await reader.ReadLineAsync(ct) is { } line)
            {
                lock (lines)
                {
                    lines.Add(line);
                }
                onLine?.Invoke(streamName, line);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on cancellation
        }
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

public sealed record ProcessResult(int ExitCode, string Stdout, string Stderr);
