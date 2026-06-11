using Microsoft.Extensions.Logging;
using Orchestrator.Application.Common;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Builds;

public sealed class TriggerBuildHandler(
    IPipelineRepository pipelines,
    IBuildRepository builds,
    IJobRepository jobs,
    IPipelineYamlParser yamlParser,
    IPipelineTriggerMatcher triggerMatcher,
    IDagEngine dagEngine,
    IConditionEvaluator conditionEvaluator,
    IDomainEventDispatcher eventDispatcher,
    ILogger<TriggerBuildHandler> logger
) : ICommandHandler<TriggerBuildCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        TriggerBuildCommand command,
        CancellationToken ct = default
    )
    {
        var pipeline =
            await pipelines.GetByIdAsync(command.PipelineId, ct)
            ?? throw new InvalidOperationException($"Pipeline {command.PipelineId} not found");

        var yamlContent = pipeline.YamlContent;
        if (string.IsNullOrWhiteSpace(yamlContent))
        {
            logger.LogInformation(
                "Pipeline {PipelineId} has no YAML content. Creating build without stages.",
                command.PipelineId
            );
            return await CreateBuildWithoutStages(pipeline, command, ct);
        }

        var pipelineDef = yamlParser.Parse(yamlContent);

        if (!triggerMatcher.Matches(
                pipelineDef.Trigger,
                command.Branch,
                command.TriggerEvent
            ))
        {
            logger.LogInformation(
                "Trigger {Event} on branch {Branch} does not match pipeline '{Pipeline}' trigger config. Skipping.",
                command.TriggerEvent,
                command.Branch,
                pipelineDef.Name
            );
            return Guid.Empty;
        }

        var build = pipeline.TriggerBuild(
            command.TriggerEvent,
            command.CommitSha,
            command.Priority
        );
        build.Start();

        var dagResult = dagEngine.BuildLayers(pipelineDef.Stages);
        var context = new BuildContext(
            command.Branch,
            command.TriggerEvent,
            command.Actor,
            pipeline.Repo,
            null
        );

        foreach (var stage in dagResult.TopologicalOrder)
        {
            var job = build.AddJob(stage.Name);

            var conditionMet = string.IsNullOrEmpty(stage.Condition)
                || conditionEvaluator.Evaluate(stage.Condition, context);

            if (!conditionMet)
            {
                job.Skip();
                logger.LogInformation(
                    "Job {JobName} skipped: condition '{Condition}' not met",
                    stage.Name,
                    stage.Condition
                );
                continue;
            }

            if (dagResult.Layers[0].Contains(stage))
            {
                job.Queue();
            }
        }

        // Advance DAG for any in-memory skipped stages
        var changed = true;
        while (changed)
        {
            changed = false;
            foreach (var pendingJob in build.Jobs.Where(j => j.Status == Domain.ValueObjects.JobStatus.Pending))
            {
                var stageDef = pipelineDef.Stages.FirstOrDefault(s => s.Name == pendingJob.StageName);
                if (stageDef == null) continue;

                var upstreamJobs = build.Jobs
                    .Where(j => stageDef.DependsOn.Contains(j.StageName))
                    .ToList();

                var allUpstreamFinished = stageDef.DependsOn.Count > 0 && upstreamJobs.All(j => j.Status is
                    Domain.ValueObjects.JobStatus.Passed or
                    Domain.ValueObjects.JobStatus.Failed or
                    Domain.ValueObjects.JobStatus.Cancelled or
                    Domain.ValueObjects.JobStatus.Skipped);

                if (allUpstreamFinished)
                {
                    var anyUpstreamFailed = upstreamJobs.Any(j => j.Status is
                        Domain.ValueObjects.JobStatus.Failed or
                        Domain.ValueObjects.JobStatus.Cancelled or
                        Domain.ValueObjects.JobStatus.Skipped);

                    if (anyUpstreamFailed)
                    {
                        pendingJob.Skip();
                        changed = true;
                    }
                    else
                    {
                        var conditionMet = string.IsNullOrEmpty(stageDef.Condition)
                            || conditionEvaluator.Evaluate(stageDef.Condition, context);

                        if (conditionMet)
                        {
                            pendingJob.Queue();
                            changed = true;
                        }
                        else
                        {
                            pendingJob.Skip();
                            changed = true;
                        }
                    }
                }
            }
        }

        if (build.AllJobsTerminal())
        {
            var anyFailed = build.Jobs.Any(j => j.Status == Domain.ValueObjects.JobStatus.Failed);
            var anyCancelled = build.Jobs.Any(j => j.Status == Domain.ValueObjects.JobStatus.Cancelled);
            var finalStatus = (anyFailed || anyCancelled) ? Domain.ValueObjects.BuildStatus.Failed : Domain.ValueObjects.BuildStatus.Passed;
            build.Complete(finalStatus);
        }

        await builds.AddAsync(build, ct);
        await eventDispatcher.DispatchAsync(build.DomainEvents, ct);
        build.ClearDomainEvents();

        logger.LogInformation(
            "Build {BuildId} created and started for pipeline '{Pipeline}' ({Stages} stages, {LayerCount} layers)",
            build.Id,
            pipelineDef.Name,
            pipelineDef.Stages.Count,
            dagResult.Layers.Count
        );

        return build.Id;
    }

    private async Task<Guid> CreateBuildWithoutStages(
        Domain.Entities.Pipeline pipeline,
        TriggerBuildCommand command,
        CancellationToken ct
    )
    {
        var build = pipeline.TriggerBuild(
            command.TriggerEvent,
            command.CommitSha,
            command.Priority
        );
        build.Start();
        await builds.AddAsync(build, ct);
        await eventDispatcher.DispatchAsync(build.DomainEvents, ct);
        build.ClearDomainEvents();
        return build.Id;
    }
}
