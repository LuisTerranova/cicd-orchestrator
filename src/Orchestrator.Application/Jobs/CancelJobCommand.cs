namespace Orchestrator.Application.Jobs;

public sealed record CancelJobCommand(Guid JobId, string Reason);
