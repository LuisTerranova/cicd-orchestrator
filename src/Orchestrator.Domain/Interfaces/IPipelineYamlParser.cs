namespace Orchestrator.Domain.Interfaces;

public interface IPipelineYamlParser
{
    PipelineDefinition Parse(string yamlContent);
}

public sealed record PipelineDefinition(
    string Name,
    TriggerConfig? Trigger,
    Dictionary<string, string> Env,
    List<StageDefinition> Stages
);

public sealed record TriggerConfig(
    string[] Branches,
    string[] Events
);

public sealed record StageDefinition(
    string Name,
    List<string> DependsOn,
    string? Condition,
    string? Image,
    TimeSpan Timeout,
    List<StepDefinition> Steps
);

public sealed record StepDefinition(
    string Name,
    string Run,
    TimeSpan? Timeout,
    string? WorkingDir,
    string? Shell,
    bool ContinueOnError,
    List<string>? Artifacts
);
