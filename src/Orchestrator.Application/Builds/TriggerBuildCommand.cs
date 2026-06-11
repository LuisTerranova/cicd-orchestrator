namespace Orchestrator.Application.Builds;

public sealed record TriggerBuildCommand(
    Guid PipelineId,
    string TriggerEvent,
    string CommitSha,
    string Actor,
    string Branch,
    int Priority = 0
);
