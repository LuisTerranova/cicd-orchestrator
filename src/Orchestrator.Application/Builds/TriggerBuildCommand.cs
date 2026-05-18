namespace Orchestrator.Application.Builds;

public sealed record TriggerBuildCommand(Guid PipelineId, string TriggerEvent, string CommitSha, int Priority = 0);
