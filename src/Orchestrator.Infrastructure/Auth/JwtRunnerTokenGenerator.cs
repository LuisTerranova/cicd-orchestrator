using System.Security.Cryptography;
using System.Text;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Infrastructure.Auth;

public class JwtRunnerTokenGenerator : IRunnerTokenGenerator
{
    private const string SecretKey =
        "c3VwZXItc2VjcmV0LWtleS1mb3ItZGV2ZWxvcG1lbnQtcHVycG9zZXMtb25seQ==";

    public string GenerateToken(Guid runnerId)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var payload = $"{runnerId}|{timestamp}";
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var encodedPayload = Convert.ToBase64String(payloadBytes);

        var keyBytes = Encoding.UTF8.GetBytes(SecretKey);
        var hash = HMACSHA256.HashData(keyBytes, payloadBytes);
        var encodedSignature = Convert.ToBase64String(hash);

        return $"{encodedPayload}.{encodedSignature}";
    }

    public bool ValidateToken(string token, out Guid runnerId)
    {
        runnerId = Guid.Empty;

        if (string.IsNullOrEmpty(token))
            return false;

        var parts = token.Split('.');
        if (parts.Length != 2)
            return false;

        try
        {
            var payloadBytes = Convert.FromBase64String(parts[0]);
            var signatureBytes = Convert.FromBase64String(parts[1]);

            var keyBytes = Encoding.UTF8.GetBytes(SecretKey);
            var expectedHash = HMACSHA256.HashData(keyBytes, payloadBytes);

            if (!CryptographicOperations.FixedTimeEquals(signatureBytes, expectedHash))
                return false;

            var payload = Encoding.UTF8.GetString(payloadBytes);
            var payloadParts = payload.Split('|');
            if (payloadParts.Length != 2)
                return false;

            return Guid.TryParse(payloadParts[0], out runnerId);
        }
        catch
        {
            return false;
        }
    }
}
