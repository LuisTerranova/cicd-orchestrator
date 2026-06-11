using Orchestrator.Infrastructure.Services;

namespace Orchestrator.Tests.Infrastructure;

public sealed class WebhookValidatorTests
{
    private readonly HmacWebhookSignatureValidator _sut = new();

    [Fact]
    public void Validate_ValidHmacSignature_ReturnsTrue()
    {
        var payload = "{\"event\":\"push\"}";
        var secret = "my-secret-key";
        var signature = ComputeHmac(payload, secret);

        var result = _sut.Validate(payload, signature, secret);

        Assert.True(result);
    }

    [Fact]
    public void Validate_InvalidSignature_ReturnsFalse()
    {
        var payload = "{\"event\":\"push\"}";
        var secret = "my-secret-key";

        var result = _sut.Validate(payload, "invalid-signature", secret);

        Assert.False(result);
    }

    [Fact]
    public void Validate_WrongSecret_ReturnsFalse()
    {
        var payload = "{\"event\":\"push\"}";
        var correctSecret = "correct-secret";
        var wrongSecret = "wrong-secret";
        var signature = ComputeHmac(payload, correctSecret);

        var result = _sut.Validate(payload, signature, wrongSecret);

        Assert.False(result);
    }

    [Fact]
    public void Validate_EmptyPayload_ReturnsCorrectResult()
    {
        var secret = "my-secret-key";
        var payload = "";
        var signature = ComputeHmac(payload, secret);

        var result = _sut.Validate(payload, signature, secret);

        Assert.True(result);
    }

    [Fact]
    public void Validate_MalformedPayload_DoesNotThrow()
    {
        var payload = "not-json-{content";
        var secret = "key";

        var signature = ComputeHmac(payload, secret);
        var result = _sut.Validate(payload, signature, secret);

        Assert.True(result);
    }

    [Fact]
    public void Validate_EmptySecret_DoesNotThrow()
    {
        var payload = "{\"event\":\"push\"}";
        var signature = ComputeHmac(payload, "");

        var result = _sut.Validate(payload, signature, "");

        Assert.True(result);
    }

    private static string ComputeHmac(string payload, string secret)
    {
        var keyBytes = System.Text.Encoding.UTF8.GetBytes(secret);
        var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
        var hash = System.Security.Cryptography.HMACSHA256.HashData(keyBytes, payloadBytes);
        return Convert.ToHexStringLower(hash);
    }
}
