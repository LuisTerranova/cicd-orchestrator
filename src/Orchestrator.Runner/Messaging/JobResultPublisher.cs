using Orchestrator.Contracts.Messages;

namespace Orchestrator.Runner.Messaging;

public sealed class JobResultPublisher
{
    public async Task PublishAsync(JobCompleted completed, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
