using Orchestrator.Contracts.Messages;

namespace Orchestrator.Runner.Execution;

public sealed class StepRunner
{
    private readonly ContainerStepRunner _containerRunner;
    private readonly NativeStepRunner _nativeRunner;

    public StepRunner(ContainerStepRunner containerRunner, NativeStepRunner nativeRunner)
    {
        _containerRunner = containerRunner;
        _nativeRunner = nativeRunner;
    }

    // Routes to ContainerStepRunner when the job specifies an image,
    // otherwise falls back to direct process execution on the host.
    public async Task<JobStepResult> RunStepAsync(
        JobStep step,
        Guid jobId,
        string workspacePath,
        Dictionary<string, string> secrets,
        string? image,
        CancellationToken ct
    )
    {
        if (!string.IsNullOrEmpty(image))
        {
            return await _containerRunner.RunAsync(step, jobId, workspacePath, secrets, image, ct);
        }

        return await _nativeRunner.RunAsync(step, jobId, workspacePath, secrets, ct);
    }
}
