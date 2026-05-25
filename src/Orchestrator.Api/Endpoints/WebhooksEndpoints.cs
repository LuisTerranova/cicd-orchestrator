using Orchestrator.Application.Webhooks;

namespace Orchestrator.Api.Endpoints;

public static class WebhooksEndpoints
{
    public static void MapWebhooksEndpoints(this IEndpointRouteBuilder app)
    {
        // POST /api/webhooks - Process incoming VCS webhooks (GitHub, GitLab, etc.)
        app.MapPost("/api/webhooks", async (ProcessWebhookCommand command, HttpContext http, CancellationToken ct) =>
        {
            var handler = http.RequestServices.GetRequiredService<ProcessWebhookHandler>();
            await handler.HandleAsync(command, ct);
            return Results.Ok();
        });
    }
}

