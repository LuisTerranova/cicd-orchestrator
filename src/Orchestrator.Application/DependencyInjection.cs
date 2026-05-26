using Microsoft.Extensions.DependencyInjection;
using Orchestrator.Application.Builds;
using Orchestrator.Application.Logs;
using Orchestrator.Application.Pipelines;
using Orchestrator.Application.Runners;

namespace Orchestrator.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register Queries (standard Scoped registration)
        services.AddScoped<GetBuildByIdQuery>();
        services.AddScoped<GetBuildsByPipelineIdQuery>();
        services.AddScoped<GetLogByJobIdQuery>();
        services.AddScoped<GetAllPipelinesQuery>();
        services.AddScoped<GetPipelineByIdQuery>();
        services.AddScoped<GetAllRunnersQuery>();
        services.AddScoped<GetRunnerByIdQuery>();

        return services;
    }
}
