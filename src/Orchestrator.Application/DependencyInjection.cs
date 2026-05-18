using Microsoft.Extensions.DependencyInjection;
using Orchestrator.Application.Builds;
using Orchestrator.Application.Jobs;
using Orchestrator.Application.Logs;
using Orchestrator.Application.Pipelines;
using Orchestrator.Application.Runners;
using Orchestrator.Application.Webhooks;

namespace Orchestrator.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
        => throw new NotImplementedException();
}
