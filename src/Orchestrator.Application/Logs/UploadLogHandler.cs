using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Logs;

public sealed class UploadLogHandler
{
    private readonly ILogRepository _logs;
    private readonly IUnitOfWork _unitOfWork;

    public UploadLogHandler(
        ILogRepository logs,
        IUnitOfWork unitOfWork)
    {
        _logs = logs;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> HandleAsync(UploadLogCommand command, CancellationToken ct = default)
    {
        var log = Domain.Entities.LogMetadata.Create(command.JobId, command.FilePath, command.LineCount, command.SizeBytes);
        await _logs.AddAsync(log, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return log.Id;
    }
}
