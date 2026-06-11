using System.Text.RegularExpressions;
using Orchestrator.Domain.Exceptions;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Infrastructure.Services;

public sealed class PipelineYamlParser : IPipelineYamlParser
{
    public PipelineDefinition Parse(string yamlContent)
    {
        if (string.IsNullOrWhiteSpace(yamlContent))
            throw new DomainException("Pipeline YAML cannot be empty.");

        var lines = yamlContent.Split('\n', StringSplitOptions.None);
        var result = new Dictionary<string, object>();
        ParseBlock(lines, 0, result, out _);

        var name = GetString(result, "name", "pipeline name");
        var trigger = ParseTrigger(result.GetValueOrDefault("trigger"));
        var env = ParseEnv(result.GetValueOrDefault("env"));
        var stagesRaw = result.GetValueOrDefault("stages");
        var stages = ParseStages(stagesRaw);

        if (stages.Count == 0)
            throw new DomainException("Pipeline must have at least one stage.");

        return new PipelineDefinition(name, trigger, env, stages);
    }

    private static TriggerConfig? ParseTrigger(object? triggerObj)
    {
        if (triggerObj is not Dictionary<string, object> trigger)
            return null;

        var branches = trigger.TryGetValue("branches", out var b)
            ? ToStringArray(b)
            : [];

        var events = trigger.TryGetValue("events", out var e)
            ? ToStringArray(e)
            : [];

        return new TriggerConfig(branches, events);
    }

    private static Dictionary<string, string> ParseEnv(object? envObj)
    {
        if (envObj is not Dictionary<string, object> env)
            return [];

        return env.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? "");
    }

    private static List<StageDefinition> ParseStages(object? stagesObj)
    {
        var stages = new List<StageDefinition>();

        if (stagesObj is not List<object> stagesList)
            throw new DomainException("'stages' must be a list.");

        foreach (var item in stagesList)
        {
            if (item is not Dictionary<string, object> stageMap)
                throw new DomainException("Each stage must be an object.");

            var name = GetString(stageMap, "name", "stage name");
            var dependsOn = stageMap.TryGetValue("depends_on", out var dep)
                ? ToStringArray(dep).ToList()
                : new List<string>();
            var condition = stageMap.TryGetValue("condition", out var cond)
                ? cond?.ToString()
                : null;
            var image = stageMap.TryGetValue("image", out var img)
                ? img?.ToString()
                : null;
            var timeout = stageMap.TryGetValue("timeout", out var t)
                ? ParseDuration(t?.ToString())
                : TimeSpan.FromMinutes(30);
            var steps = ParseSteps(stageMap.GetValueOrDefault("steps"), name);

            stages.Add(new StageDefinition(name, dependsOn, condition, image, timeout, steps));
        }

        return stages;
    }

    private static List<StepDefinition> ParseSteps(object? stepsObj, string stageName)
    {
        var steps = new List<StepDefinition>();

        if (stepsObj is not List<object> stepsList)
            throw new DomainException($"Stage '{stageName}' must have a 'steps' list.");

        foreach (var item in stepsList)
        {
            if (item is not Dictionary<string, object> stepMap)
                throw new DomainException($"Each step in stage '{stageName}' must be an object.");

            var name = GetString(stepMap, "name", $"step in stage '{stageName}'");
            var run = GetString(stepMap, "run", $"step '{name}'");
            var timeout = stepMap.TryGetValue("timeout", out var t)
                ? ParseDuration(t?.ToString())
                : (TimeSpan?)null;
            var workingDir = stepMap.TryGetValue("working_dir", out var w)
                ? w?.ToString()
                : null;
            var shell = stepMap.TryGetValue("shell", out var s)
                ? s?.ToString()
                : null;
            var continueOnError = stepMap.TryGetValue("continue_on_error", out var coe)
                && coe is bool coeBool && coeBool;
            var artifacts = stepMap.TryGetValue("artifacts", out var a)
                ? ToStringArray(a)
                : null;

            steps.Add(new StepDefinition(name, run, timeout, workingDir, shell, continueOnError, artifacts?.ToList()));
        }

        if (steps.Count == 0)
            throw new DomainException($"Stage '{stageName}' must have at least one step.");

        return steps;
    }

    private static string GetString(Dictionary<string, object> map, string key, string context)
    {
        if (!map.TryGetValue(key, out var value) || value is not string s || string.IsNullOrWhiteSpace(s))
            throw new DomainException($"'{key}' is required for {context}.");
        return s;
    }

    private static string[] ToStringArray(object value)
    {
        if (value is List<object> list)
            return list.Select(x => x?.ToString() ?? "").ToArray();
        if (value is string s)
            return [s];
        return [];
    }

    private static TimeSpan ParseDuration(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return TimeSpan.FromMinutes(30);

        var match = Regex.Match(input, @"^(\d+)([smh])$");
        if (!match.Success)
            return TimeSpan.FromMinutes(30);

        var value = int.Parse(match.Groups[1].Value);
        return match.Groups[2].Value switch
        {
            "s" => TimeSpan.FromSeconds(value),
            "m" => TimeSpan.FromMinutes(value),
            "h" => TimeSpan.FromHours(value),
            _ => TimeSpan.FromMinutes(30),
        };
    }

    private static void ParseBlock(string[] lines, int start, Dictionary<string, object> result, out int nextIndex)
    {
        nextIndex = start;

        for (var i = start; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmed = line.TrimStart();

            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
                continue;

            var indent = line.Length - trimmed.Length;

            if (indent == 0 && i > start && trimmed.StartsWith('-'))
            {
                nextIndex = i;
                return;
            }

            if (indent == 0 && i > start && !trimmed.Contains(':') && !trimmed.StartsWith('-'))
            {
                nextIndex = i;
                return;
            }

            if (trimmed.StartsWith('-') && indent > 0)
            {
                var itemContent = trimmed.TrimStart('-', ' ');
                var colonIdx = itemContent.IndexOf(':');
                if (colonIdx > 0)
                {
                    var key = itemContent[..colonIdx].Trim();
                    var value = itemContent[(colonIdx + 1)..].Trim();
                    if (!result.ContainsKey(key))
                        result[key] = value;
                }
                continue;
            }

            var colonPos = trimmed.IndexOf(':');
            if (colonPos <= 0)
                continue;

            var keyName = trimmed[..colonPos].Trim();
            var afterColon = trimmed[(colonPos + 1)..].Trim();

            if (string.IsNullOrEmpty(afterColon))
            {
                var subBlock = new Dictionary<string, object>();
                var subItems = new List<object>();
                var inList = false;

                for (var j = i + 1; j < lines.Length; j++)
                {
                    var subLine = lines[j];
                    var subTrimmed = subLine.TrimStart();
                    var subIndent = subLine.Length - subTrimmed.Length;

                    if (subIndent <= indent || string.IsNullOrWhiteSpace(subTrimmed))
                    {
                        nextIndex = j;
                        break;
                    }

                    if (subTrimmed.StartsWith('-'))
                    {
                        inList = true;
                        var listContent = subTrimmed.TrimStart('-', ' ');

                        var subColon = listContent.IndexOf(':');
                        if (subColon > 0)
                        {
                            var itemDict = new Dictionary<string, object>();
                            var itemKey = listContent[..subColon].Trim();
                            var itemValue = listContent[(subColon + 1)..].Trim();
                            itemDict[itemKey] = itemValue;

                            var innerIndent = subLine.Length - subTrimmed.Length;
                            for (var k = j + 1; k < lines.Length; k++)
                            {
                                var innerLine = lines[k];
                                var innerTrimmed = innerLine.TrimStart();
                                var innerIndent2 = innerLine.Length - innerTrimmed.Length;

                                if (innerIndent2 <= innerIndent || string.IsNullOrWhiteSpace(innerTrimmed))
                                {
                                    j = k - 1;
                                    break;
                                }

                                var innerColon = innerTrimmed.IndexOf(':');
                                if (innerColon > 0)
                                {
                                    var ik = innerTrimmed[..innerColon].Trim();
                                    var iv = innerTrimmed[(innerColon + 1)..].Trim();
                                    itemDict[ik] = iv;
                                }

                                j = k;
                            }

                            subItems.Add(itemDict);
                        }
                        else
                        {
                            subItems.Add(listContent);
                        }
                    }
                    else
                    {
                        var subColon = subTrimmed.IndexOf(':');
                        if (subColon > 0)
                        {
                            var sk = subTrimmed[..subColon].Trim();
                            var sv = subTrimmed[(subColon + 1)..].Trim();
                            subBlock[sk] = sv;
                        }

                        nextIndex = j + 1;
                    }
                }

                if (inList)
                    result[keyName] = subItems;
                else if (subBlock.Count > 0)
                    result[keyName] = subBlock;
                else
                    result[keyName] = "";
            }
            else
            {
                result[keyName] = afterColon;
            }
        }

        nextIndex = lines.Length;
    }
}
