using Orchestrator.Api.Extensions;
using Orchestrator.Application.Common;
using Orchestrator.Application.Webhooks;

namespace Orchestrator.Api.Endpoints;

public class WebhooksEndpoints : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/webhooks",
            async (
                WebhookRequest request,
                ICommandHandler<ProcessWebhookCommand> processWebhookHandler,
                CancellationToken ct
            ) =>
            {
                var command = new ProcessWebhookCommand(
                    request.Payload,
                    request.Signature,
                    request.Secret
                );
                await processWebhookHandler.HandleAsync(command, ct);
                return Results.Ok();
            }
        );
    }
}
