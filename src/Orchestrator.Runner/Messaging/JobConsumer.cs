using MassTransit;
using Microsoft.Extensions.Logging;
using Orchestrator.Contracts.Messages;
using Orchestrator.Runner.Agent;
using Orchestrator.Runner.Execution;

namespace Orchestrator.Runner.Messaging;

public sealed class JobConsumer : IConsumer<JobQueued>
{
    private readonly RunnerState _state;
    private readonly JobExecutor _executor;
    private readonly JobResultPublisher _publisher;
    private readonly ILogger<JobConsumer> _logger;

    public JobConsumer(
        RunnerState state,
        JobExecutor executor,
        JobResultPublisher publisher,
        ILogger<JobConsumer> logger
    )
    {
        _state = state;
        _executor = executor;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<JobQueued> context)
    {
        var job = context.Message;

        if (_state.Draining || !_state.TryAcquireSlot())
        {
            _logger.LogWarning("Job {JobId} rejected — no slot available or draining", job.JobId);
            return;
        }

        _state.SetActiveJob(job.JobId);
        _logger.LogInformation(
            "Starting job {JobId} ({Pipeline}/{Stage})",
            job.JobId,
            job.PipelineName,
            job.StageName
        );

        try
        {
            var result = await _executor.ExecuteAsync(job, context.CancellationToken);
            await _publisher.PublishAsync(result, context.CancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Job {JobId} was cancelled", job.JobId);
        }
        finally
        {
            _state.ClearActiveJob(job.JobId);
            _state.ReleaseSlot();
        }
    }
}
