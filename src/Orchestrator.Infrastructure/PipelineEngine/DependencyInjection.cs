using Microsoft.Extensions.DependencyInjection;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Domain.Services;
using Orchestrator.Infrastructure.Services;

namespace Orchestrator.Infrastructure.PipelineEngine;

public static class DependencyInjection
{
    public static IServiceCollection AddPipelineEngine(this IServiceCollection services)
    {
        services.AddSingleton<IDagEngine, DagEngine>();
        services.AddScoped<IPipelineYamlParser, YamlDotNetParser>();
        services.AddSingleton<IConditionEvaluator, ConditionEvaluator>();
        services.AddSingleton<IPipelineTriggerMatcher, PipelineTriggerMatcher>();
        services.AddScoped<IJobDispatcher, HttpJobDispatcher>();

        return services;
    }
}
