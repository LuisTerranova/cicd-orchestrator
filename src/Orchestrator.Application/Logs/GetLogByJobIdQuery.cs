using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Logs;

public sealed class GetLogByJobIdQuery(ILogRepository logs)
{
    public async Task<Domain.Entities.LogMetadata?> HandleAsync(
        Guid jobId,
        CancellationToken ct = default
    ) => await logs.GetByJobIdAsync(jobId, ct);
}
