namespace Orchestrator.Application.Webhooks;

public sealed record ProcessWebhookCommand(string Payload, string Signature, string Secret);
