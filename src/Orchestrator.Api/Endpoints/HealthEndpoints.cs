using System.Diagnostics;
using Orchestrator.Api.Extensions;
using Orchestrator.Infrastructure.Persistence;

namespace Orchestrator.Api.Endpoints;

public class HealthEndpoints : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/health", CheckHealthAsync);
        app.MapGet("/ready", ReadinessProbeAsync);
    }

    private static async Task<IResult> CheckHealthAsync(
        OrchestratorDbContext dbContext,
        CancellationToken ct
    )
    {
        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync(ct);
            if (canConnect)
                return Results.Ok(new HealthResponse("healthy", DateTime.UtcNow));

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

    private static async Task<IResult> ReadinessProbeAsync(
        OrchestratorDbContext db,
        CancellationToken ct
    )
    {
        var checks = new Dictionary<string, object>();

        try
        {
            var sw = Stopwatch.StartNew();
            await db.Database.CanConnectAsync(ct);
            checks["database"] = new { status = "healthy", latency = $"{sw.ElapsedMilliseconds}ms" };
        }
        catch
        {
            checks["database"] = new { status = "unhealthy" };
        }

        var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.Name == "/");
        checks["disk"] = drive is not null
            ? new { status = "healthy", available = $"{drive.AvailableFreeSpace / 1024 / 1024 / 1024}GB" }
            : new { status = "unknown" };

        var healthy = checks.Values.All(v => v is not null);
        return healthy
            ? Results.Ok(new { status = "ready", checks })
            : Results.Json(new { status = "not_ready", checks }, statusCode: 503);
    }
}
