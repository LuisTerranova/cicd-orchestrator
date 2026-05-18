namespace Orchestrator.Application.Jobs;

public sealed record AssignJobCommand(Guid JobId, Guid RunnerId);
