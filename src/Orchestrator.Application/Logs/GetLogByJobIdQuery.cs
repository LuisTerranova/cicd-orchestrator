using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Logs;

public sealed class GetLogByJobIdQuery
{
    private readonly ILogRepository _logs;

    public GetLogByJobIdQuery(ILogRepository logs) { }

    public Task<Domain.Entities.LogMetadata?> HandleAsync(Guid jobId, CancellationToken ct = default)
        => throw new NotImplementedException();
}
