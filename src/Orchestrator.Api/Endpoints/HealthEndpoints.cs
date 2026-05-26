using Orchestrator.Api.Extensions;
using Orchestrator.Infrastructure.Persistence;

namespace Orchestrator.Api.Endpoints;

public class HealthEndpoints : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        // Database connectivity checks for monitoring/orchestration
        app.MapGet("/api/health", CheckHealthAsync);
    }

    private static async Task<IResult> CheckHealthAsync(
        OrchestratorDbContext dbContext,
        CancellationToken ct
    )
    {
        try
        {
            // Verify if the database is reachable and accepting requests
            var canConnect = await dbContext.Database.CanConnectAsync(ct);
            if (canConnect)
            {
                return Results.Ok(new HealthResponse("healthy", DateTime.UtcNow));
            }

            return Results.Json(
                new HealthResponse("unhealthy", DateTime.UtcNow),
                statusCode: StatusCodes.Status503ServiceUnavailable
            );
        }
        catch (Exception)
        {
            return Results.Json(
                new HealthResponse("unhealthy", DateTime.UtcNow),
                statusCode: StatusCodes.Status503ServiceUnavailable
            );
        }
    }
}
