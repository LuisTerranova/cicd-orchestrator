using Microsoft.Extensions.Logging;
using Orchestrator.Application.Common;
using Orchestrator.Application.Builds;

namespace Orchestrator.Application.Webhooks;

public sealed class ProcessWebhookHandler(
    ICommandHandler<TriggerBuildCommand, Guid> triggerBuildHandler,
    ILogger<ProcessWebhookHandler> logger
) : ICommandHandler<ProcessWebhookCommand>
{
    public async Task HandleAsync(ProcessWebhookCommand command, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Processing webhook triggering build for pipeline {PipelineId} on branch {Branch}", 
            command.PipelineId, 
            command.Branch
        );

        var triggerCommand = new TriggerBuildCommand(
            command.PipelineId,
            command.EventType,
            command.CommitSha,
            command.Actor,
            command.Branch
        );

        await triggerBuildHandler.HandleAsync(triggerCommand, ct);
    }
}
