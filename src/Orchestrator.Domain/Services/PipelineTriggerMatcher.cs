using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Domain.Services;

public sealed class PipelineTriggerMatcher : IPipelineTriggerMatcher
{
    public bool Matches(TriggerConfig? trigger, string branch, string eventType)
    {
        if (trigger is null)
            return true;

        if (trigger.Events is { Length: > 0 } && !trigger.Events.Contains(eventType))
            return false;

        if (trigger.Branches is { Length: > 0 } && !MatchesBranch(branch, trigger.Branches))
            return false;

        return true;
    }

    private static bool MatchesBranch(string branch, string[] patterns)
    {
        foreach (var pattern in patterns)
        {
            if (pattern == "*" || pattern == "**")
                return true;

            if (pattern.EndsWith('/'))
            {
                var prefix = pattern.TrimEnd('/');
                if (branch.StartsWith(prefix + "/", StringComparison.Ordinal))
                    return true;
                continue;
            }

            if (pattern.EndsWith("/*", StringComparison.Ordinal))
            {
                var prefix = pattern[..^2];
                if (branch.StartsWith(prefix + "/", StringComparison.Ordinal) &&
                    !branch[(prefix.Length + 1)..].Contains('/'))
                    return true;
                continue;
            }

            if (pattern.EndsWith("/**", StringComparison.Ordinal))
            {
                var prefix = pattern[..^3];
                if (branch.StartsWith(prefix + "/", StringComparison.Ordinal))
                    return true;
                continue;
            }

            if (branch == pattern)
                return true;
        }

        return false;
    }
}
