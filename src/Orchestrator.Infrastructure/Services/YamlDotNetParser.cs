using System.Text.RegularExpressions;
using Orchestrator.Domain.Exceptions;
using Orchestrator.Domain.Interfaces;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Orchestrator.Infrastructure.Services;

public sealed class YamlDotNetParser : IPipelineYamlParser
{
    private readonly IDeserializer _deserializer;

    public YamlDotNetParser()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public PipelineDefinition Parse(string yamlContent)
    {
        if (string.IsNullOrWhiteSpace(yamlContent))
            throw new DomainException("Pipeline YAML cannot be empty.");

        var raw = _deserializer.Deserialize<RawPipeline>(yamlContent)
            ?? throw new DomainException("Failed to parse pipeline YAML.");

        if (string.IsNullOrWhiteSpace(raw.Name))
            throw new DomainException("Pipeline 'name' is required.");

        if (raw.Stages is null || raw.Stages.Count == 0)
            throw new DomainException("Pipeline must have at least one stage.");

        var trigger = raw.Trigger is not null
            ? new TriggerConfig(
                raw.Trigger.Branches ?? [],
                raw.Trigger.Events ?? []
            )
            : null;

        var env = raw.Env ?? new Dictionary<string, string>();

        var stages = raw.Stages.Select(s =>
        {
            if (string.IsNullOrWhiteSpace(s.Name))
                throw new DomainException("Each stage must have a 'name'.");

            if (s.Steps is null || s.Steps.Count == 0)
                throw new DomainException(
                    $"Stage '{s.Name}' must have at least one step.");

            var steps = s.Steps.Select(st =>
            {
                if (string.IsNullOrWhiteSpace(st.Name))
                    throw new DomainException(
                        $"Each step in stage '{s.Name}' must have a 'name'.");
                if (string.IsNullOrWhiteSpace(st.Run))
                    throw new DomainException(
                        $"Step '{st.Name}' in stage '{s.Name}' must have a 'run' command.");

                return new StepDefinition(
                    st.Name,
                    st.Run,
                    ParseDuration(st.Timeout),
                    st.WorkingDir,
                    st.Shell ?? "sh",
                    st.ContinueOnError,
                    st.Artifacts?.ToList()
                );
            }).ToList();

            return new StageDefinition(
                s.Name,
                s.DependsOn ?? [],
                s.Condition,
                s.Image,
                ParseDuration(s.Timeout) ?? TimeSpan.FromMinutes(30),
                steps
            );
        }).ToList();

        return new PipelineDefinition(raw.Name, trigger, env, stages);
    }

    private static TimeSpan? ParseDuration(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var match = Regex.Match(input, @"^(\d+)([smh])$");
        if (!match.Success)
            return null;

        var value = int.Parse(match.Groups[1].Value);
        return match.Groups[2].Value switch
        {
            "s" => TimeSpan.FromSeconds(value),
            "m" => TimeSpan.FromMinutes(value),
            "h" => TimeSpan.FromHours(value),
            _ => null,
        };
    }

    private sealed class RawPipeline
    {
        public string? Name { get; set; }
        public RawTrigger? Trigger { get; set; }
        public Dictionary<string, string>? Env { get; set; }
        public List<RawStage>? Stages { get; set; }
    }

    private sealed class RawTrigger
    {
        public string[]? Branches { get; set; }
        public string[]? Events { get; set; }
    }

    private sealed class RawStage
    {
        public string? Name { get; set; }
        public List<string>? DependsOn { get; set; }
        public string? Condition { get; set; }
        public string? Image { get; set; }
        public string? Timeout { get; set; }
        public List<RawStep>? Steps { get; set; }
    }

    private sealed class RawStep
    {
        public string? Name { get; set; }
        public string? Run { get; set; }
        public string? Timeout { get; set; }
        public string? WorkingDir { get; set; }
        public string? Shell { get; set; }
        public bool ContinueOnError { get; set; }
        public string[]? Artifacts { get; set; }
    }
}
