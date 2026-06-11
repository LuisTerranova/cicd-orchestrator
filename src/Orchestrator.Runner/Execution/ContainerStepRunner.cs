using Microsoft.Extensions.Logging;
using Orchestrator.Contracts.Messages;

namespace Orchestrator.Runner.Execution;

public sealed class ContainerStepRunner
{
    private readonly Container.PodmanCli _podmanCli;
    private readonly TempScriptWriter _scriptWriter;
    private readonly ILogger<ContainerStepRunner> _logger;

    public ContainerStepRunner(
        Container.PodmanCli podmanCli,
        TempScriptWriter scriptWriter,
        ILogger<ContainerStepRunner> logger
    )
    {
        _podmanCli = podmanCli;
        _scriptWriter = scriptWriter;
        _logger = logger;
    }

    private static string Sanitize(string name) =>
        string.Join("_", name.Split(Path.GetInvalidFileNameChars()));

    // Full container lifecycle: pull image → write step script → create container
    // (with workspace mount) → start → exec script → return result.
    // The script path inside the container is derived from /workspace mount point.
    // Container cleanup (stop + remove) is guaranteed via a finally block.
    public async Task<JobStepResult> RunAsync(
        JobStep step,
        Guid jobId,
        string workspacePath,
        Dictionary<string, string> secrets,
        string image,
        CancellationToken ct
    )
    {
        var startedAt = DateTime.UtcNow;
        string? containerId = null;

        try
        {
            await _podmanCli.PullAsync(image, ct);

            _scriptWriter.WriteScript(step, jobId, workspacePath);
            // Translate host script path to container-internal path via workspace mount.
            var containerScriptPath = $"/workspace/steps/{Sanitize(step.Name)}.sh";

            var spec = new Container.ContainerSpec(
                JobId: jobId,
                ContainerName: $"step-{jobId:N}-{Sanitize(step.Name)}",
                Image: image,
                WorkspacePath: workspacePath,
                SecretsPath: "/run/secrets"
            );

            containerId = await _podmanCli.CreateAsync(image, spec, ct);
            await _podmanCli.StartAsync(containerId, ct);

            var shell = step.Shell ?? "bash";
            var exitCode = await _podmanCli.ExecAsync(containerId, shell, containerScriptPath, ct);

            return new JobStepResult(
                step.Name,
                exitCode == 0 ? "passed" : "failed",
                exitCode,
                DateTime.UtcNow - startedAt
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Container step {StepName} failed", step.Name);
            return new JobStepResult(step.Name, "failed", -1, DateTime.UtcNow - startedAt);
        }
        finally
        {
            if (containerId is not null)
            {
                try { await _podmanCli.StopAsync(containerId, TimeSpan.FromSeconds(10), CancellationToken.None); }
                catch { /* best effort */ }
                try { await _podmanCli.RemoveAsync(containerId, CancellationToken.None); }
                catch { /* best effort */ }
            }
        }
    }
}
