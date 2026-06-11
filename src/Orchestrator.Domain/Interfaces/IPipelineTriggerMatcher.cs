namespace Orchestrator.Domain.Interfaces;

public interface IPipelineTriggerMatcher
{
    bool Matches(TriggerConfig? trigger, string branch, string eventType);
}
