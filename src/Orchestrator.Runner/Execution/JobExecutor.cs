using Microsoft.Extensions.Logging;
using Orchestrator.Contracts.Messages;
using Orchestrator.Runner.Agent;
using Orchestrator.Runner.Artifacts;
using Orchestrator.Runner.Configuration;
using Orchestrator.Runner.Logging;
using Orchestrator.Runner.Registration;

namespace Orchestrator.Runner.Execution;

public sealed class JobExecutor
{
    private readonly RunnerState _state;
    private readonly StepRunner _stepRunner;
    private readonly ProcessInvoker _process;
    private readonly ArtifactUploader _artifactUploader;
    private readonly LogCapturer _logCapturer;
    private readonly RunnerOptions _options;
    private readonly CredentialStore _credentials;
    private readonly ILogger<JobExecutor> _logger;

    public JobExecutor(
        RunnerState state,
        StepRunner stepRunner,
        ProcessInvoker process,
        ArtifactUploader artifactUploader,
        LogCapturer logCapturer,
        RunnerOptions options,
        CredentialStore credentials,
        ILogger<JobExecutor> logger
    )
    {
        _state = state;
        _stepRunner = stepRunner;
        _process = process;
        _artifactUploader = artifactUploader;
        _logCapturer = logCapturer;
        _options = options;
        _credentials = credentials;
        _logger = logger;
    }

    // Full job lifecycle:
    //   1. Create workspace directory
    //   2. Git clone (shallow, with optional commit checkout)
    //   3. Start log capture with secret masking
    //   4. Run each step sequentially; abort on first failure
    //   5. Upload all files from workspace as artifacts
    //   6. Build and return JobCompleted result
    //   7. Finalize logs and clean up workspace (always runs)
    public async Task<JobCompleted> ExecuteAsync(JobQueued job, CancellationToken ct)
    {
        var startedAt = DateTime.UtcNow;
        var workspacePath = Path.Combine(_options.WorkspacePath, job.JobId.ToString());
        var stepResults = new List<JobStepResult>();
        var artifacts = new List<ArtifactInfo>();

        try
        {
            Directory.CreateDirectory(workspacePath);

            var cloneResult = await CloneRepository(job, workspacePath, ct);
            if (cloneResult.ExitCode != 0)
            {
                return BuildResult(
                    job,
                    startedAt,
                    "failed",
                    cloneResult.ExitCode,
                    [],
                    [],
                    cloneResult.Stderr
                );
            }

            _logCapturer.StartCapture(job.JobId, job.Secrets);

            foreach (var step in job.Steps)
            {
                // Per-step timeout linkage: job-level timeout + external cancellation.
                using var timeoutCts = new CancellationTokenSource(job.Timeout);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    ct,
                    timeoutCts.Token
                );

                _logger.LogInformation("Running step {StepName}", step.Name);
                _logCapturer.CaptureLine(job.JobId, "step", $"Starting step '{step.Name}'");

                var stepResult = await _stepRunner.RunStepAsync(
                    step,
                    job.JobId,
                    workspacePath,
                    job.Secrets,
                    job.Image,
                    linkedCts.Token
                );

                stepResults.Add(stepResult);
                _logCapturer.CaptureLine(
                    job.JobId,
                    "step",
                    $"Step '{step.Name}': {stepResult.Status} (exit code {stepResult.ExitCode})"
                );

                if (stepResult.ExitCode != 0)
                {
                    _logger.LogWarning("Step {StepName} failed — aborting", step.Name);
                    break;
                }
            }

            artifacts.AddRange(await UploadArtifacts(job.BuildId, workspacePath, ct));

            var overallStatus = stepResults.Any(s => s.ExitCode != 0) ? "failed" : "passed";
            var exitCode = stepResults.Count > 0 ? stepResults[^1].ExitCode : 0;

            return BuildResult(
                job,
                startedAt,
                overallStatus,
                exitCode,
                [.. stepResults],
                [.. artifacts],
                null
            );
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Job {JobId} was cancelled", job.JobId);
            return BuildResult(
                job,
                startedAt,
                "cancelled",
                -1,
                [.. stepResults],
                [.. artifacts],
                "Job was cancelled"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job {JobId} failed", job.JobId);
            return BuildResult(
                job,
                startedAt,
                "failed",
                -1,
                [.. stepResults],
                [.. artifacts],
                ex.Message
            );
        }
        finally
        {
            // Always flush logs and clean up, even on failure or cancellation.
            await _logCapturer.CompleteAsync(job.JobId, CancellationToken.None);

            try
            {
                if (Directory.Exists(workspacePath))
                {
                    Directory.Delete(workspacePath, recursive: true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clean workspace {Path}", workspacePath);
            }
        }
    }

    private async Task<ProcessResult> CloneRepository(
        JobQueued job,
        string workspacePath,
        CancellationToken ct
    )
    {
        _logger.LogInformation("Cloning {Repo} @ {Ref}", job.RepoUrl, job.Ref);

        var args = new List<string>
        {
            "clone",
            "--depth",
            job.CloneDepth.ToString(),
            "--branch",
            job.Ref,
            job.RepoUrl,
            workspacePath,
        };

        var result = await _process.RunAsync("git", [.. args], ct);
        if (result.ExitCode != 0)
            return result;

        if (!string.IsNullOrEmpty(job.CommitSha))
        {
            _logger.LogInformation("Checking out {Sha}", job.CommitSha);
            result = await _process.RunAsync(
                "git",
                ["-C", workspacePath, "checkout", job.CommitSha],
                ct
            );
        }

        return result;
    }

    private async Task<List<ArtifactInfo>> UploadArtifacts(
        Guid buildId,
        string workspacePath,
        CancellationToken ct
    )
    {
        var artifacts = new List<ArtifactInfo>();

        if (!Directory.Exists(workspacePath))
            return artifacts;

        foreach (var file in Directory.GetFiles(workspacePath, "*", SearchOption.AllDirectories))
        {
            try
            {
                await _artifactUploader.UploadAsync(buildId, file, ct);
                var info = new FileInfo(file);
                artifacts.Add(
                    new ArtifactInfo(Path.GetRelativePath(workspacePath, file), file, info.Length)
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to upload artifact {File}", file);
            }
        }

        return artifacts;
    }

    // Loads the runner ID from credential store to populate the JobCompleted record.
    private JobCompleted BuildResult(
        JobQueued job,
        DateTime startedAt,
        string status,
        int exitCode,
        JobStepResult[] steps,
        ArtifactInfo[] artifacts,
        string? error
    )
    {
        var creds = _credentials.LoadAsync().GetAwaiter().GetResult();
        var runnerId = creds?.RunnerId ?? string.Empty;
        var completedAt = DateTime.UtcNow;

        return new JobCompleted(
            job.Version,
            job.JobId,
            runnerId,
            status,
            exitCode,
            startedAt,
            completedAt,
            completedAt - startedAt,
            steps,
            artifacts,
            error
        );
    }
}
