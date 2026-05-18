namespace Orchestrator.Application.Jobs;

public sealed record CompleteJobCommand(Guid JobId, int ExitCode);
