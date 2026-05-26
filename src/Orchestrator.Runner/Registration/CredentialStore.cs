using System.Runtime.Versioning;
using System.Text.Json;
using Orchestrator.Runner.Configuration;

namespace Orchestrator.Runner.Registration;

[SupportedOSPlatform("linux")]
public sealed class CredentialStore
{
    private readonly string _path;

    public CredentialStore(RunnerOptions options)
    {
        var raw = options.CredentialsPath.Replace(
            "~",
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        );
        _path = raw;
    }

    public async Task<(string RunnerId, string Secret)?> LoadAsync()
    {
        if (!File.Exists(_path))
            return null;

        var json = await File.ReadAllTextAsync(_path);
        var creds = JsonSerializer.Deserialize<CredentialFile>(json);
        return creds is not null ? (creds.RunnerId, creds.Secret) : null;
    }

    public async Task SaveAsync(string runnerId, string secret)
    {
        var dir = Path.GetDirectoryName(_path)!;
        Directory.CreateDirectory(dir);

        var creds = new CredentialFile { RunnerId = runnerId, Secret = secret };
        var json = JsonSerializer.Serialize(creds);
        await File.WriteAllTextAsync(_path, json);

        File.SetUnixFileMode(_path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
    }

    public bool Exists() => File.Exists(_path);

    private sealed record CredentialFile
    {
        public string RunnerId { get; init; } = string.Empty;
        public string Secret { get; init; } = string.Empty;
    }
}
