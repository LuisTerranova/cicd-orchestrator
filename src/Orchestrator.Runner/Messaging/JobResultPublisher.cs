using MassTransit;
using Orchestrator.Contracts.Messages;

namespace Orchestrator.Runner.Messaging;

public sealed class JobResultPublisher
{
    private readonly IBus _bus;

    public JobResultPublisher(IBus bus)
    {
        _bus = bus;
    }

    public async Task PublishAsync(JobCompleted completed, CancellationToken ct)
    {
        await _bus.Publish(completed, ct);
    }
}
