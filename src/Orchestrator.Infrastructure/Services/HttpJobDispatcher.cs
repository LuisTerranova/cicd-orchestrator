using Microsoft.Extensions.Logging;
using MassTransit;
using Orchestrator.Contracts.Messages;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Domain.ValueObjects;
using Orchestrator.Infrastructure.Persistence;

namespace Orchestrator.Infrastructure.Services;

public sealed class HttpJobDispatcher : IJobDispatcher
{
    private readonly IRunnerRepository _runners;
    private readonly IJobRepository _jobs;
    private readonly IPipelineRepository _pipelines;
    private readonly IPipelineYamlParser _yamlParser;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly OrchestratorDbContext _dbContext;
    private readonly ILogger<HttpJobDispatcher> _logger;

    public HttpJobDispatcher(
        IRunnerRepository runners,
        IJobRepository jobs,
        IPipelineRepository pipelines,
        IPipelineYamlParser yamlParser,
        IPublishEndpoint publishEndpoint,
        OrchestratorDbContext dbContext,
        ILogger<HttpJobDispatcher> logger
    )
    {
        _runners = runners;
        _jobs = jobs;
        _pipelines = pipelines;
        _yamlParser = yamlParser;
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task DispatchAsync(Job job, Build build, CancellationToken ct = default)
    {
        // If the job already has a runner assigned, it means it was already dispatched
        if (job.RunnerId != null)
        {
            _logger.LogInformation("Job {JobId} already assigned to runner {RunnerId}. Skipping immediate dispatch.", job.Id, job.RunnerId);
            return;
        }

        var pipeline = await _pipelines.GetByIdAsync(build.PipelineId, ct);
        if (pipeline == null || string.IsNullOrWhiteSpace(pipeline.YamlContent))
        {
            _logger.LogWarning("Pipeline not found or has no YAML content for job {JobId}", job.Id);
            return;
        }

        StageDefinition? stage = null;
        PipelineDefinition? pipelineDef = null;
        try
        {
            pipelineDef = _yamlParser.Parse(pipeline.YamlContent);
            stage = pipelineDef.Stages.FirstOrDefault(s => s.Name == job.StageName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse pipeline YAML for immediate dispatch of job {JobId}", job.Id);
            return;
        }

        if (stage == null) return;

        var requiredLabels = stage.Image != null ? new[] { stage.Image } : Array.Empty<string>();

        // Find an Idle runner that matches the required labels
        var idleRunners = await _runners.GetByStatusAsync(RunnerStatus.Idle, ct);
        var matchingRunner = idleRunners.FirstOrDefault(r =>
            requiredLabels.All(label => r.HasLabel(label))
        );

        if (matchingRunner == null)
        {
            _logger.LogInformation("No idle runner found matching labels for job {JobId}. Job will be queued for background dispatch.", job.Id);
            return;
        }

        try
        {
            // Assign the job to the runner
            job.AssignTo(matchingRunner);
            matchingRunner.GoBusy();

            // Save changes
            await _jobs.UpdateAsync(job, ct);
            await _runners.UpdateAsync(matchingRunner, ct);
            await _dbContext.SaveChangesAsync(ct);

            // Build the message
            var steps = stage.Steps
                .Select(s => new JobStep(
                    s.Name,
                    s.Run,
                    s.Timeout,
                    s.WorkingDir ?? "/workspace/repo",
                    s.Shell ?? "sh"
                ))
                .ToArray();

            var env = pipelineDef?.Env ?? new Dictionary<string, string>();

            var message = new JobQueued(
                JobId: job.Id,
                BuildId: build.Id,
                PipelineName: pipeline.Name,
                StageName: job.StageName,
                RepoUrl: pipeline.Repo,
                Ref: build.CommitSha,
                CommitSha: build.CommitSha,
                CloneDepth: 1,
                Image: stage.Image ?? "",
                Timeout: stage.Timeout,
                Steps: steps,
                Env: env,
                Secrets: new Dictionary<string, string>(), // Can load from pipeline secrets later
                RegistryAuth: null,
                Labels: matchingRunner.Labels,
                Priority: build.Priority,
                Version: job.Version,
                WorkspacePath: ""
            );

            // Publish to the specific runner's routing key
            await _publishEndpoint.Publish(message, context =>
            {
                context.SetRoutingKey($"job.{matchingRunner.Name}");
            }, ct);

            _logger.LogInformation("Successfully assigned and immediately dispatched job {JobId} to runner {RunnerName}", job.Id, matchingRunner.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to immediately dispatch job {JobId}", job.Id);
            throw;
        }
    }
}
