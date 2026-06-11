using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Orchestrator.Api.Extensions;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration config)
    {
        services.AddOpenTelemetry()
            .WithTracing(b => b
                .AddAspNetCoreInstrumentation()
                .AddOtlpExporter())
            .WithMetrics(b => b
                .AddAspNetCoreInstrumentation()
                .AddPrometheusExporter());

        return services;
    }
}
