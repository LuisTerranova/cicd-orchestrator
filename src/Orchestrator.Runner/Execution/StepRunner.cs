using Orchestrator.Contracts.Messages;

namespace Orchestrator.Runner.Execution;

public sealed class StepRunner
{
    public async Task<JobStepResult> RunStepAsync(JobStep step, Guid jobId, string workspacePath,
        Dictionary<string, string> secrets, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
