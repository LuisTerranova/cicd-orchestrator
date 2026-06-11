namespace Orchestrator.Application.Pipelines;

public sealed record UpdatePipelineCommand(Guid Id, string Name, string Repo, string Branch, string YamlPath);
