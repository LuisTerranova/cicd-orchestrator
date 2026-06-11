using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MassTransit;
using Orchestrator.Contracts.Messages;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Domain.ValueObjects;
using Orchestrator.Infrastructure.Persistence;

namespace Orchestrator.Infrastructure.Services;

public sealed class JobDispatcherBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JobDispatcherBackgroundService> _logger;

    public JobDispatcherBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<JobDispatcherBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("JobDispatcherBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
                var runnerRepository = scope.ServiceProvider.GetRequiredService<IRunnerRepository>();
                var buildRepository = scope.ServiceProvider.GetRequiredService<IBuildRepository>();
                var pipelineRepository = scope.ServiceProvider.GetRequiredService<IPipelineRepository>();
                var yamlParser = scope.ServiceProvider.GetRequiredService<IPipelineYamlParser>();
                var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
                var dbContext = scope.ServiceProvider.GetRequiredService<OrchestratorDbContext>();

                // Get all queued jobs that are not assigned to a runner yet
                var queuedJobs = (await jobRepository.GetByStatusAsync(JobStatus.Queued, stoppingToken))
                    .Where(j => j.RunnerId == null)
                    .ToList();

                foreach (var job in queuedJobs)
                {
                    var build = await buildRepository.GetByIdAsync(job.BuildId, stoppingToken);
                    if (build == null) continue;

                    var pipeline = await pipelineRepository.GetByIdAsync(build.PipelineId, stoppingToken);
                    if (pipeline == null) continue;

                    if (string.IsNullOrWhiteSpace(pipeline.YamlContent)) continue;

                    StageDefinition? stage = null;
                    PipelineDefinition? pipelineDef = null;
                    try
                    {
                        pipelineDef = yamlParser.Parse(pipeline.YamlContent);
                        stage = pipelineDef.Stages.FirstOrDefault(s => s.Name == job.StageName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to parse pipeline YAML for job {JobId}", job.Id);
                        continue;
                    }

                    if (stage == null) continue;

                    var requiredLabels = stage.Image != null ? new[] { stage.Image } : Array.Empty<string>();

                    // Find an Idle runner that matches the required labels
                    var idleRunners = await runnerRepository.GetByStatusAsync(RunnerStatus.Idle, stoppingToken);
                    var matchingRunner = idleRunners.FirstOrDefault(r =>
                        requiredLabels.All(label => r.HasLabel(label))
                    );

                    if (matchingRunner != null)
                    {
                        // Assign the job to the runner
                        job.AssignTo(matchingRunner);
                        matchingRunner.GoBusy();

                        // Save changes
                        await jobRepository.UpdateAsync(job, stoppingToken);
                        await runnerRepository.UpdateAsync(matchingRunner, stoppingToken);
                        await dbContext.SaveChangesAsync(stoppingToken);

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
                        await publishEndpoint.Publish(message, context =>
                        {
                            context.SetRoutingKey($"job.{matchingRunner.Name}");
                        }, stoppingToken);

                        _logger.LogInformation("Assigned and dispatched job {JobId} to runner {RunnerName}", job.Id, matchingRunner.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in JobDispatcherBackgroundService");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
