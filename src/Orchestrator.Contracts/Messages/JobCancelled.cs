namespace Orchestrator.Contracts.Messages;

public sealed record JobCancelled(Guid JobId, string Reason);
