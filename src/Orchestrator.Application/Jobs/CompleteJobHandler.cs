using Orchestrator.Application.Common;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Domain.ValueObjects;

namespace Orchestrator.Application.Jobs;

public sealed class CompleteJobHandler(
    IJobRepository jobs,
    IBuildRepository builds,
    IPipelineRepository pipelines,
    IPipelineYamlParser yamlParser,
    IConditionEvaluator conditionEvaluator,
    IDomainEventDispatcher eventDispatcher
) : ICommandHandler<CompleteJobCommand>
{
    public async Task HandleAsync(CompleteJobCommand command, CancellationToken ct = default)
    {
        var job = await jobs.GetByIdAsync(command.JobId, ct)
            ?? throw new InvalidOperationException($"Job {command.JobId} not found");

        if (job.Status is JobStatus.Passed or JobStatus.Failed or JobStatus.Cancelled or JobStatus.Skipped)
        {
            return;
        }

        job.Complete(command.ExitCode);
        await jobs.UpdateAsync(job, ct);

        var build = await builds.GetByIdAsync(job.BuildId, ct)
            ?? throw new InvalidOperationException($"Build {job.BuildId} not found");

        var pipeline = await pipelines.GetByIdAsync(build.PipelineId, ct)
            ?? throw new InvalidOperationException($"Pipeline {build.PipelineId} not found");

        if (!string.IsNullOrWhiteSpace(pipeline.YamlContent))
        {
            var pipelineDef = yamlParser.Parse(pipeline.YamlContent);
            var buildContext = new BuildContext("main", build.TriggerEvent, "system", pipeline.Repo, null);

            var changed = true;
            while (changed)
            {
                changed = false;
                var currentJobs = await jobs.GetByBuildIdAsync(build.Id, ct);
                var pendingJobs = currentJobs.Where(j => j.Status == JobStatus.Pending).ToList();

                foreach (var pendingJob in pendingJobs)
                {
                    var stageDef = pipelineDef.Stages.FirstOrDefault(s => s.Name == pendingJob.StageName);
                    if (stageDef == null) continue;

                    var upstreamJobs = currentJobs
                        .Where(j => stageDef.DependsOn.Contains(j.StageName))
                        .ToList();

                    var allUpstreamFinished = stageDef.DependsOn.Count > 0 && upstreamJobs.All(j => j.Status is
                        JobStatus.Passed or
                        JobStatus.Failed or
                        JobStatus.Cancelled or
                        JobStatus.Skipped);

                    if (allUpstreamFinished)
                    {
                        var anyUpstreamFailed = upstreamJobs.Any(j => j.Status is
                            JobStatus.Failed or
                            JobStatus.Cancelled or
                            JobStatus.Skipped);

                        if (anyUpstreamFailed)
                        {
                            pendingJob.Skip();
                            await jobs.UpdateAsync(pendingJob, ct);
                            changed = true;
                        }
                        else
                        {
                            var conditionMet = string.IsNullOrEmpty(stageDef.Condition)
                                || conditionEvaluator.Evaluate(stageDef.Condition, buildContext);

                            if (conditionMet)
                            {
                                pendingJob.Queue();
                                await jobs.UpdateAsync(pendingJob, ct);
                                changed = true;
                            }
                            else
                            {
                                pendingJob.Skip();
                                await jobs.UpdateAsync(pendingJob, ct);
                                changed = true;
                            }
                        }
                    }
                }
            }
        }

        var finalJobs = await jobs.GetByBuildIdAsync(build.Id, ct);
        var allJobsFinished = finalJobs.Count > 0 && finalJobs.All(j => j.Status is
            JobStatus.Passed or
            JobStatus.Failed or
            JobStatus.Cancelled or
            JobStatus.Skipped);

        if (allJobsFinished)
        {
            var anyFailed = finalJobs.Any(j => j.Status == JobStatus.Failed);
            var anyCancelled = finalJobs.Any(j => j.Status == JobStatus.Cancelled);

            var finalBuildStatus = BuildStatus.Passed;
            if (anyFailed || anyCancelled)
            {
                finalBuildStatus = BuildStatus.Failed;
            }

            build.Complete(finalBuildStatus);
            await builds.UpdateAsync(build, ct);
        }

        await eventDispatcher.DispatchAsync(job.DomainEvents, ct);
        job.ClearDomainEvents();

        await eventDispatcher.DispatchAsync(build.DomainEvents, ct);
        build.ClearDomainEvents();

        var reloadedJobs = await jobs.GetByBuildIdAsync(build.Id, ct);
        foreach (var rj in reloadedJobs)
        {
            if (rj.DomainEvents.Count > 0)
            {
                await eventDispatcher.DispatchAsync(rj.DomainEvents, ct);
                rj.ClearDomainEvents();
            }
        }
    }
}
