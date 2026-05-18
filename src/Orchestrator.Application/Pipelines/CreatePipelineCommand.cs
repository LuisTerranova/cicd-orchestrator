namespace Orchestrator.Application.Pipelines;

public sealed record CreatePipelineCommand(string Name, string Repo, string Branch, string YamlPath);
