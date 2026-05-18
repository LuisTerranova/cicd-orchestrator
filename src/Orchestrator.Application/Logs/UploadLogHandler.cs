using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Logs;

public sealed class UploadLogHandler
{
    private readonly ILogRepository _logs;
    private readonly IUnitOfWork _unitOfWork;

    public UploadLogHandler(
        ILogRepository logs,
        IUnitOfWork unitOfWork) { }

    public Task<Guid> HandleAsync(UploadLogCommand command, CancellationToken ct = default)
        => throw new NotImplementedException();
}
