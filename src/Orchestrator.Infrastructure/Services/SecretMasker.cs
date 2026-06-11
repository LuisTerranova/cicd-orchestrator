namespace Orchestrator.Infrastructure.Services;

public sealed class SecretMasker
{
    public string Mask(string line, Dictionary<string, string> secrets)
    {
        foreach (var (key, value) in secrets)
        {
            if (!string.IsNullOrEmpty(value) && value.Length >= 4)
                line = line.Replace(value, "***", StringComparison.Ordinal);
        }
        return line;
    }

    public string MaskAll(string line, Dictionary<string, string> secrets)
    {
        var ordered = secrets
            .Where(s => !string.IsNullOrEmpty(s.Value) && s.Value.Length >= 4)
            .OrderByDescending(s => s.Value.Length)
            .ToList();

        foreach (var (_, value) in ordered)
            line = line.Replace(value, "***", StringComparison.Ordinal);

        return line;
    }
}
