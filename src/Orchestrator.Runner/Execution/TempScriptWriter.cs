using System.Runtime.Versioning;
using Orchestrator.Contracts.Messages;

namespace Orchestrator.Runner.Execution;

[SupportedOSPlatform("linux")]
public sealed class TempScriptWriter
{
    public string WriteScript(JobStep step, Guid jobId, string workspacePath)
    {
        var scriptDir = Path.Combine(workspacePath, "steps");
        Directory.CreateDirectory(scriptDir);

        var scriptPath = Path.Combine(scriptDir, $"{Sanitize(step.Name)}.sh");
        var shebang = step.Shell switch
        {
            "bash" => "#!/bin/bash",
            "pwsh" => "#!/usr/bin/env pwsh",
            _ => "#!/bin/sh",
        };

        File.WriteAllText(scriptPath, $"{shebang}\nset -e\n{step.Run}\n");
        File.SetUnixFileMode(
            scriptPath,
            UnixFileMode.UserRead
                | UnixFileMode.UserExecute
                | UnixFileMode.GroupRead
                | UnixFileMode.GroupExecute
        );

        return scriptPath;
    }

    private static string Sanitize(string name) =>
        string.Join("_", name.Split(Path.GetInvalidFileNameChars()));
}
