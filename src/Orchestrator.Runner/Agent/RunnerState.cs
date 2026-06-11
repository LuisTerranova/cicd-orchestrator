using System.Collections.Concurrent;

namespace Orchestrator.Runner.Agent;

public sealed class RunnerState
{
    private readonly SemaphoreSlim _slot;
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _jobCts = new();

    public RunnerState()
    {
        _slot = new SemaphoreSlim(1, 1);
    }

    public RunnerState(int concurrency)
    {
        _slot = new SemaphoreSlim(concurrency, concurrency);
    }

    public bool Draining { get; set; }

    public Guid[] ActiveJobIds => [.. _jobCts.Keys];

    public bool TryAcquireSlot() => _slot.Wait(0);

    public void ReleaseSlot() => _slot.Release();

    // Creates per-job CancellationTokenSource so CancelJob can signal a specific job.
    public CancellationToken SetActiveJob(Guid jobId)
    {
        // Dispose existing CTS if re-registering the same job ID
        if (_jobCts.TryRemove(jobId, out var existing))
        {
            existing.Dispose();
        }

        var cts = new CancellationTokenSource();
        _jobCts[jobId] = cts;
        return cts.Token;
    }

    public void ClearActiveJob(Guid jobId)
    {
        if (_jobCts.TryRemove(jobId, out var cts))
        {
            cts.Dispose();
        }
    }

    // Called by CancellationConsumer to abort a running job via its linked CTS token.
    public void CancelJob(Guid jobId)
    {
        if (_jobCts.TryGetValue(jobId, out var cts))
        {
            cts.Cancel();
        }
    }

    // Polling loop used during graceful shutdown — waits for all active jobs to finish.
    public async Task WaitForActiveJobs(CancellationToken ct)
    {
        while (_jobCts.Count > 0 && !ct.IsCancellationRequested)
        {
            await Task.Delay(500, ct);
        }
    }
}
