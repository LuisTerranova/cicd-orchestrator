using Orchestrator.Domain.Entities;

namespace Orchestrator.Domain.Interfaces;

public interface IJobDispatcher
{
    Task DispatchAsync(Job job, Build build, CancellationToken ct = default);
}
