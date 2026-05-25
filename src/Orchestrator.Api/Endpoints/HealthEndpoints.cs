using Orchestrator.Api.Extensions;

namespace Orchestrator.Api.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        // Simple health check endpoint for monitoring/orchestration
        app.MapGet("/api/health", () => Results.Ok(new HealthResponse("healthy", DateTime.UtcNow)));
    }
}

