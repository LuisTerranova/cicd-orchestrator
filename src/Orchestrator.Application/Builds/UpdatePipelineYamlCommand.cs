namespace Orchestrator.Application.Builds;

public sealed record UpdatePipelineYamlCommand(Guid PipelineId, string YamlContent);
