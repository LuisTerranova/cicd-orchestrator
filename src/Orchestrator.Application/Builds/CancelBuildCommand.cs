namespace Orchestrator.Application.Builds;

public sealed record CancelBuildCommand(Guid BuildId, string Reason = "user_requested");
