using Orchestrator.Contracts.Messages;

namespace Orchestrator.Runner.Execution;

public sealed class ContainerStepRunner
{
    public ContainerStepRunner(Container.PodmanCli podmanCli)
    {
        throw new NotImplementedException();
    }

    public async Task<JobStepResult> RunAsync(JobStep step, Guid jobId, string workspacePath,
        Dictionary<string, string> secrets, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
