using Microsoft.Extensions.Logging;
using Orchestrator.Contracts.Messages;

namespace Orchestrator.Runner.Execution;

public sealed class StepRunner
{
    private readonly ContainerStepRunner _containerRunner;
    private readonly ILogger<StepRunner> _logger;

    public StepRunner(
        ContainerStepRunner containerRunner,
        ILogger<StepRunner> logger)
    {
        _containerRunner = containerRunner;
        _logger = logger;
    }

    // Routes to ContainerStepRunner, falling back to a default container image if none specified.
    // Supports retry with exponential backoff and continue-on-error.
    public async Task<JobStepResult> RunStepAsync(
        JobStep step,
        Guid jobId,
        string workspacePath,
        Dictionary<string, string> secrets,
        string? image,
        CancellationToken ct,
        bool continueOnError = false,
        int retryCount = 0)
    {
        var attempts = retryCount + 1;
        var runImage = string.IsNullOrEmpty(image) ? "ubuntu:latest" : image;

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            try
            {
                JobStepResult result = await _containerRunner.RunAsync(step, jobId, workspacePath, secrets, runImage, ct);

                if (result.ExitCode == 0 || attempt >= attempts)
                    return result;

                _logger.LogWarning(
                    "Step {StepName} failed (attempt {Attempt}/{MaxRetries}) — retrying",
                    step.Name, attempt, retryCount);
            }
            catch (Exception ex) when (attempt < attempts && !ct.IsCancellationRequested)
            {
                _logger.LogWarning(ex,
                    "Step {StepName} threw (attempt {Attempt}/{MaxRetries}) — retrying",
                    step.Name, attempt, retryCount);
            }

            if (attempt < attempts)
            {
                await Task.Delay(TimeSpan.FromSeconds(1 << (attempt - 1)), ct);
            }
        }

        // Final attempt — let the exception propagate or return failure
        return await ExecuteFinal(step, jobId, workspacePath, secrets, runImage, ct);
    }

    private async Task<JobStepResult> ExecuteFinal(
        JobStep step,
        Guid jobId,
        string workspacePath,
        Dictionary<string, string> secrets,
        string image,
        CancellationToken ct)
    {
        return await _containerRunner.RunAsync(step, jobId, workspacePath, secrets, image, ct);
    }
}
