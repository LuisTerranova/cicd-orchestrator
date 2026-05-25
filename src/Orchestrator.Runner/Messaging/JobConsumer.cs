using MassTransit;
using Orchestrator.Contracts.Messages;

namespace Orchestrator.Runner.Messaging;

public sealed class JobConsumer : IConsumer<JobQueued>
{
    public async Task Consume(ConsumeContext<JobQueued> context)
    {
        throw new NotImplementedException();
    }
}
