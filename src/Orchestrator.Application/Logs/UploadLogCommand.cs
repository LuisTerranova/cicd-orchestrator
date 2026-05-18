namespace Orchestrator.Application.Logs;

public sealed record UploadLogCommand(Guid JobId, string FilePath, int LineCount, long SizeBytes);
