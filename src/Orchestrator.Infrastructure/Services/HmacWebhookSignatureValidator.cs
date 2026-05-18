using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Infrastructure.Services;

public class HmacWebhookSignatureValidator : IWebhookSignatureValidator
{
    public bool Validate(string payload, string signature, string secret)
        => throw new NotImplementedException();
}
