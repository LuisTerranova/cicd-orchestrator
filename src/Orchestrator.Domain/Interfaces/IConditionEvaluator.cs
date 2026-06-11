namespace Orchestrator.Domain.Interfaces;

public interface IConditionEvaluator
{
    bool Evaluate(string expression, BuildContext context);
}

public sealed record BuildContext(
    string Branch,
    string Event,
    string Actor,
    string Repo,
    string? Tag
)
{
    public bool IsPr => Event == "pull_request";
    public bool IsMain => Branch == "main";
}
