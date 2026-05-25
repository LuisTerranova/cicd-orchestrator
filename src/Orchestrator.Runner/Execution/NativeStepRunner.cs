using Orchestrator.Contracts.Messages;

namespace Orchestrator.Runner.Execution;

public sealed class NativeStepRunner
{
    public NativeStepRunner(ProcessInvoker processInvoker)
    {
        throw new NotImplementedException();
    }

    public async Task<JobStepResult> RunAsync(JobStep step, Guid jobId, string workspacePath,
        Dictionary<string, string> secrets, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
