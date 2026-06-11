using System.Reflection;
using Orchestrator.Api.Endpoints;

namespace Orchestrator.Api.Extensions;

public static class EndpointExtensions
{
    private static readonly Type[] EndpointTypes;

    static EndpointExtensions()
    {
        EndpointTypes = typeof(IEndpoint)
            .Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false }
                && typeof(IEndpoint).IsAssignableFrom(t))
            .ToArray();
    }

    public static IServiceCollection AddEndpoints(this IServiceCollection services)
    {
        foreach (var type in EndpointTypes)
            services.AddScoped(type);

        return services;
    }

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        foreach (var type in EndpointTypes)
        {
            var mapMethod = type.GetMethod(
                nameof(IEndpoint.Map),
                BindingFlags.Public | BindingFlags.Static
            );
            mapMethod?.Invoke(null, [app]);
        }

        return app;
    }
}
