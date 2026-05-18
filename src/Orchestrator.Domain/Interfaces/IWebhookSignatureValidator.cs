namespace Orchestrator.Domain.Interfaces;

public interface IWebhookSignatureValidator
{
    bool Validate(string payload, string signature, string secret);
}
