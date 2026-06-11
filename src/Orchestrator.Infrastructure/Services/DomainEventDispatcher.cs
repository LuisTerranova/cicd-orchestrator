using Microsoft.Extensions.Logging;
using Orchestrator.Domain;
using Orchestrator.Domain.Events;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Infrastructure.Services;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly ILogger<DomainEventDispatcher> _logger;
    private readonly IJobDispatcher _jobDispatcher;
    private readonly IJobRepository _jobRepository;
    private readonly IBuildRepository _buildRepository;

    public DomainEventDispatcher(
        ILogger<DomainEventDispatcher> logger,
        IJobDispatcher jobDispatcher,
        IJobRepository jobRepository,
        IBuildRepository buildRepository
    )
    {
        _logger = logger;
        _jobDispatcher = jobDispatcher;
        _jobRepository = jobRepository;
        _buildRepository = buildRepository;
    }

    public async Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> events,
        CancellationToken ct = default
    )
    {
        foreach (var e in events)
        {
            _logger.LogInformation("Domain event: {EventType}", e.GetType().Name);

            switch (e)
            {
                case JobQueuedEvent queued:
                    await OnJobQueuedAsync(queued, ct);
                    break;
            }
        }
    }

    private async Task OnJobQueuedAsync(JobQueuedEvent ev, CancellationToken ct)
    {
        var job = await _jobRepository.GetByIdAsync(ev.JobId, ct);
        if (job is null)
        {
            _logger.LogWarning("Job {JobId} not found for dispatch.", ev.JobId);
            return;
        }

        var build = await _buildRepository.GetByIdAsync(ev.BuildId, ct);
        if (build is null)
        {
            _logger.LogWarning("Build {BuildId} not found for job dispatch.", ev.BuildId);
            return;
        }

        await _jobDispatcher.DispatchAsync(job, build, ct);
    }
}
