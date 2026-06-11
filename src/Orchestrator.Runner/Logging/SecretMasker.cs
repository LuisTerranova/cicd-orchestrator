using System.Security.Cryptography;
using System.Text;

namespace Orchestrator.Runner.Logging;

public sealed class SecretMasker
{
    private readonly Dictionary<string, string> _registeredSecrets = new();
    private readonly object _lock = new();

    public void Register(Dictionary<string, string> secrets)
    {
        lock (_lock)
        {
            foreach (var (key, value) in secrets)
            {
                if (!string.IsNullOrEmpty(value) && value.Length >= 4)
                {
                    _registeredSecrets[key] = value;
                }
            }
        }
    }

    public string Mask(string line, Dictionary<string, string> secrets)
    {
        var sb = new StringBuilder(line);
        var combined = GetCombinedSecrets(secrets);
        foreach (var value in combined)
        {
            if (value.Length < 4)
                continue;
            sb.Replace(value, "***");
        }
        return sb.ToString();
    }

    // Compares two strings using fixed-time comparison to prevent timing side-channels.
    public bool SensitiveEquals(string a, string b)
    {
        var aBytes = Encoding.UTF8.GetBytes(a ?? string.Empty);
        var bBytes = Encoding.UTF8.GetBytes(b ?? string.Empty);
        return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }

    private List<string> GetCombinedSecrets(Dictionary<string, string> perCall)
    {
        lock (_lock)
        {
            var values = new List<string>(_registeredSecrets.Values.Count + perCall.Count);
            values.AddRange(_registeredSecrets.Values);
            foreach (var kvp in perCall)
            {
                if (!string.IsNullOrEmpty(kvp.Value) && kvp.Value.Length >= 4)
                    values.Add(kvp.Value);
            }
            return values;
        }
    }
}
