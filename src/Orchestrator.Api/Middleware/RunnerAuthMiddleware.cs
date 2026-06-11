using System.Net;
using System.Text.Json;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Api.Middleware;

public sealed class RunnerAuthMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly PathString[] AnonymousPaths =
    [
        "/api/health",
        "/ready",
        "/api/v1/webhooks",
        "/api/v1/runners/register",
        "/api/v1/runners/token",
    ];

    private static readonly PathString[] ProtectedPrefixes =
    [
        "/api/v1/jobs",
        "/api/v1/runners",
        "/api/v1/builds",
        "/api/v1/pipelines",
    ];

    public RunnerAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IRunnerTokenGenerator tokenGenerator)
    {
        var path = context.Request.Path;

        if (RequiresAuth(path))
        {
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                await WriteUnauthorized(context, "Missing or invalid Authorization header");
                return;
            }

            var token = authHeader["Bearer ".Length..];
            if (!tokenGenerator.ValidateToken(token, out var runnerId))
            {
                await WriteUnauthorized(context, "Invalid or expired token");
                return;
            }

            context.Items["RunnerId"] = runnerId;
        }

        await _next(context);
    }

    private static bool RequiresAuth(PathString path)
    {
        if (AnonymousPaths.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)))
            return false;

        return ProtectedPrefixes.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));
    }

    private static Task WriteUnauthorized(HttpContext context, string message)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        context.Response.ContentType = "application/json";
        var body = JsonSerializer.Serialize(new
        {
            error = new
            {
                code = "UNAUTHORIZED",
                message
            }
        });
        return context.Response.WriteAsync(body);
    }
}
