using System.Security.Cryptography;
using System.Text;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Infrastructure.Services;

public class HmacWebhookSignatureValidator : IWebhookSignatureValidator
{
    public bool Validate(string payload, string signature, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var hash = HMACSHA256.HashData(keyBytes, payloadBytes);
        var computed = Convert.ToHexStringLower(hash);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computed),
            Encoding.UTF8.GetBytes(signature));
    }
}
