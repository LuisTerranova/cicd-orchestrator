namespace Orchestrator.Runner.Logging;

public sealed class SecretMasker
{
    public string Mask(string line, Dictionary<string, string> secrets)
    {
        foreach (var (key, value) in secrets)
        {
            if (!string.IsNullOrEmpty(value))
            {
                line = line.Replace(value, "***", StringComparison.Ordinal);
            }
        }

        return line;
    }
}
