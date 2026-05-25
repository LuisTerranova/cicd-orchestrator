using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Logs;

public sealed class GetLogByJobIdQuery
{
    private readonly ILogRepository _logs;

    public GetLogByJobIdQuery(ILogRepository logs)
    {
        _logs = logs;
    }

    public async Task<Domain.Entities.LogMetadata?> HandleAsync(Guid jobId, CancellationToken ct = default)
        => await _logs.GetByJobIdAsync(jobId, ct);
}
