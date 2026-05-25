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
    {
        services.AddScoped<TriggerBuildHandler>();
        services.AddScoped<GetBuildByIdQuery>();
        services.AddScoped<GetBuildsByPipelineIdQuery>();
        services.AddScoped<AssignJobHandler>();
        services.AddScoped<CancelJobHandler>();
        services.AddScoped<CompleteJobHandler>();
        services.AddScoped<GetLogByJobIdQuery>();
        services.AddScoped<UploadLogHandler>();
        services.AddScoped<CreatePipelineHandler>();
        services.AddScoped<GetAllPipelinesQuery>();
        services.AddScoped<GetPipelineByIdQuery>();
        services.AddScoped<GetAllRunnersQuery>();
        services.AddScoped<GetRunnerByIdQuery>();
        services.AddScoped<RegisterRunnerHandler>();
        services.AddScoped<ProcessWebhookHandler>();
        return services;
    }
}
