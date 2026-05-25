using Microsoft.Extensions.Hosting;

namespace Orchestrator.Runner.Agent;

public sealed class RunnerAgent : IHostedLifecycleService
{
    public async Task StartAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task StopAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task StartingAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task StartedAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task StoppingAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task StoppedAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
