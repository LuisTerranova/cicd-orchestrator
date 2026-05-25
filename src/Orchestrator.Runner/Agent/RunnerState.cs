namespace Orchestrator.Runner.Agent;

public sealed class RunnerState
{
    public RunnerState()
    {
        throw new NotImplementedException();
    }

    public bool Draining { get; set; }

    public bool TryAcquireSlot()
    {
        throw new NotImplementedException();
    }

    public void ReleaseSlot()
    {
        throw new NotImplementedException();
    }

    public void SetActiveJob(Guid jobId)
    {
        throw new NotImplementedException();
    }

    public void ClearActiveJob(Guid jobId)
    {
        throw new NotImplementedException();
    }

    public async Task WaitForActiveJobs(CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
