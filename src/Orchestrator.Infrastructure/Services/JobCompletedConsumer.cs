using MassTransit;
using Orchestrator.Application.Common;
using Orchestrator.Application.Jobs;
using Orchestrator.Contracts.Messages;

namespace Orchestrator.Infrastructure.Services;

public sealed class JobCompletedConsumer(ICommandHandler<CompleteJobCommand> completeJobHandler)
    : IConsumer<JobCompleted>
{
    public async Task Consume(ConsumeContext<JobCompleted> context)
    {
        var msg = context.Message;
        var command = new CompleteJobCommand(msg.JobId, msg.ExitCode);
        await completeJobHandler.HandleAsync(command, context.CancellationToken);
    }
}
