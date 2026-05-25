using Microsoft.Extensions.Hosting;

namespace Orchestrator.Runner.Container;

public sealed class ContainerCleanupService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }

    public async Task StopAndRemoveAsync(string containerId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
