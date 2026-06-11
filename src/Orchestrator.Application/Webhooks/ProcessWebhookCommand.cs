namespace Orchestrator.Application.Webhooks;

public sealed record ProcessWebhookCommand(
    Guid PipelineId,
    string Branch,
    string EventType,
    string CommitSha,
    string Actor
);
