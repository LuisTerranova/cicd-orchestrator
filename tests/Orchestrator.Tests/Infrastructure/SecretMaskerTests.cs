using Orchestrator.Infrastructure.Services;

namespace Orchestrator.Tests.Infrastructure;

public sealed class SecretMaskerTests
{
    private readonly SecretMasker _sut = new();

    [Fact]
    public void Mask_ReplacesSecretValue()
    {
        var secrets = new Dictionary<string, string> { ["TOKEN"] = "supersecret123" };
        var result = _sut.Mask("my token is supersecret123", secrets);
        Assert.Equal("my token is ***", result);
    }

    [Fact]
    public void Mask_ShortSecretIgnored()
    {
        var secrets = new Dictionary<string, string> { ["KEY"] = "ab" };
        var result = _sut.Mask("value ab found", secrets);
        Assert.Equal("value ab found", result);
    }

    [Fact]
    public void Mask_SecretAtLeast4Chars_IsMasked()
    {
        var secrets = new Dictionary<string, string> { ["KEY"] = "abcd" };
        var result = _sut.Mask("value abcd found", secrets);
        Assert.Equal("value *** found", result);
    }

    [Fact]
    public void Mask_SecretNotPresent_NoChange()
    {
        var secrets = new Dictionary<string, string> { ["TOKEN"] = "secret123" };
        var result = _sut.Mask("clean line", secrets);
        Assert.Equal("clean line", result);
    }

    [Fact]
    public void MaskAll_OverlappingSecrets_LongerWins()
    {
        var secrets = new Dictionary<string, string>
        {
            ["SHORT"] = "abc",
            ["LONG"] = "abcdef",
        };
        var result = _sut.MaskAll("token abcdef here", secrets);
        Assert.Equal("token *** here", result);
    }

    [Fact]
    public void Mask_NullOrEmptySecrets_Skipped()
    {
        var secrets = new Dictionary<string, string> { ["TOKEN"] = "" };
        var result = _sut.Mask("nothing", secrets);
        Assert.Equal("nothing", result);
    }

    [Fact]
    public void Mask_MultipleSecrets_AllReplaced()
    {
        var secrets = new Dictionary<string, string>
        {
            ["API_KEY"] = "key1234",
            ["DB_PASS"] = "dbpass!",
        };
        var result = _sut.Mask("api=key1234 db=dbpass!", secrets);
        Assert.Equal("api=*** db=***", result);
    }
}
