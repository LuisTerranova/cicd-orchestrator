using Orchestrator.Application.Common;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Webhooks;

public sealed class ProcessWebhookHandler
{
    private readonly IWebhookSignatureValidator _signatureValidator;
    private readonly IWebhookDispatcher _webhookDispatcher;
    private readonly IBuildRepository _builds;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public ProcessWebhookHandler(
        IWebhookSignatureValidator signatureValidator,
        IWebhookDispatcher webhookDispatcher,
        IBuildRepository builds,
        IDomainEventDispatcher eventDispatcher)
    {
        _signatureValidator = signatureValidator;
        _webhookDispatcher = webhookDispatcher;
        _builds = builds;
        _eventDispatcher = eventDispatcher;
    }

    public async Task HandleAsync(ProcessWebhookCommand command, CancellationToken ct = default)
    {
        if (!_signatureValidator.Validate(command.Payload, command.Signature, command.Secret))
            throw new InvalidOperationException("Invalid webhook signature");

        await _webhookDispatcher.DispatchAsync(command.Payload, new { received = true });
    }
}
