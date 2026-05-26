using Orchestrator.Application.Common;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Webhooks;

public sealed class ProcessWebhookHandler(
    IWebhookSignatureValidator signatureValidator,
    IWebhookDispatcher webhookDispatcher
) : ICommandHandler<ProcessWebhookCommand>
{
    public async Task HandleAsync(ProcessWebhookCommand command, CancellationToken ct = default)
    {
        if (!signatureValidator.Validate(command.Payload, command.Signature, command.Secret))
            throw new InvalidOperationException("Invalid webhook signature");

        await webhookDispatcher.DispatchAsync(command.Payload, new { received = true });
    }
}
