using MassTransit;
using Orchestrator.Contracts.Messages;

namespace Orchestrator.Runner.Messaging;

public sealed class CancellationConsumer : IConsumer<JobCancelled>
{
    public async Task Consume(ConsumeContext<JobCancelled> context)
    {
        throw new NotImplementedException();
    }
}
