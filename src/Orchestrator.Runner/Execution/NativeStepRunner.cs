using Microsoft.Extensions.Logging;
using Orchestrator.Contracts.Messages;

namespace Orchestrator.Runner.Execution;

public sealed class NativeStepRunner
{
    private readonly ProcessInvoker _processInvoker;
    private readonly TempScriptWriter _scriptWriter;
    private readonly ILogger<NativeStepRunner> _logger;

    public NativeStepRunner(
        ProcessInvoker processInvoker,
        TempScriptWriter scriptWriter,
        ILogger<NativeStepRunner> logger)
    {
        _processInvoker = processInvoker;
        _scriptWriter = scriptWriter;
        _logger = logger;
    }

    public async Task<JobStepResult> RunAsync(JobStep step, Guid jobId, string workspacePath,
        Dictionary<string, string> secrets, CancellationToken ct)
    {
        var startedAt = DateTime.UtcNow;

        try
        {
            var scriptPath = _scriptWriter.WriteScript(step, jobId, workspacePath);
            var shell = step.Shell ?? "bash";
            var result = await _processInvoker.RunAsync(shell, [scriptPath], ct);

            return new JobStepResult(
                step.Name,
                result.ExitCode == 0 ? "passed" : "failed",
                result.ExitCode,
                DateTime.UtcNow - startedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Native step {StepName} failed", step.Name);
            return new JobStepResult(step.Name, "failed", -1, DateTime.UtcNow - startedAt);
        }
    }
}
