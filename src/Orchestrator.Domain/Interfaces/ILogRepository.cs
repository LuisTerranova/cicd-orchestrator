using Orchestrator.Domain.Entities;

namespace Orchestrator.Domain.Interfaces;

public interface ILogRepository
{
    Task<LogMetadata?> GetByJobIdAsync(Guid jobId, CancellationToken ct = default);
    Task AddAsync(LogMetadata log, CancellationToken ct = default);
    Task UpdateAsync(LogMetadata log, CancellationToken ct = default);
}
