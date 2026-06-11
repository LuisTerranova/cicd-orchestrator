namespace Orchestrator.Contracts.Messages;

public sealed record JobQueued(
    Guid JobId,
    Guid BuildId,
    string PipelineName,
    string StageName,
    string RepoUrl,
    string Ref,
    string CommitSha,
    int CloneDepth,
    string Image,
    TimeSpan Timeout,
    JobStep[] Steps,
    Dictionary<string, string> Env,
    Dictionary<string, string> Secrets,
    RegistryAuth? RegistryAuth,
    string[] Labels,
    int Priority,
    int Version,
    string WorkspacePath,
    string MessageId = "",
    DateTime Timestamp = default,
    string TraceId = ""
);

public sealed record JobStep(
    string Name,
    string Run,
    TimeSpan? Timeout,
    string WorkingDir,
    string Shell
);

public sealed record RegistryAuth(string Server, string Username, string Password);
