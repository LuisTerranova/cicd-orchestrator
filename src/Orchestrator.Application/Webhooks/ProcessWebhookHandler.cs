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
        IDomainEventDispatcher eventDispatcher) { }

    public Task HandleAsync(ProcessWebhookCommand command, CancellationToken ct = default)
        => throw new NotImplementedException();
}
