using Orchestrator.Application.Common;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Logs;

public sealed class UploadLogHandler(ILogRepository logs) : ICommandHandler<UploadLogCommand, Guid>
{
    public async Task<Guid> HandleAsync(UploadLogCommand command, CancellationToken ct = default)
    {
        var log = Domain.Entities.LogMetadata.Create(
            command.JobId,
            command.FilePath,
            command.LineCount,
            command.SizeBytes
        );
        await logs.AddAsync(log, ct);
        return log.Id;
    }
}
