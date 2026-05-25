using Orchestrator.Contracts.Messages;

namespace Orchestrator.Runner.Execution;

public sealed class JobExecutor
{
    public async Task<JobCompleted> ExecuteAsync(JobQueued job, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
