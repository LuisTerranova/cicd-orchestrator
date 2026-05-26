using MassTransit;
using Microsoft.Extensions.Logging;
using Orchestrator.Contracts.Messages;
using Orchestrator.Runner.Agent;

namespace Orchestrator.Runner.Messaging;

public sealed class CancellationConsumer : IConsumer<JobCancelled>
{
    private readonly RunnerState _state;
    private readonly ILogger<CancellationConsumer> _logger;

    public CancellationConsumer(RunnerState state, ILogger<CancellationConsumer> logger)
    {
        _state = state;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<JobCancelled> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "Cancellation requested for job {JobId}: {Reason}",
            msg.JobId,
            msg.Reason
        );
        _state.CancelJob(msg.JobId);
    }
}
