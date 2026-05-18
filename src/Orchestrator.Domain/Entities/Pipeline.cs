using Orchestrator.Domain.Exceptions;

namespace Orchestrator.Domain.Entities;

public class Pipeline : Entity
{
    public required string Name { get; private set; }
    public required string Repo { get; private set; }
    public string Branch { get; private set; } = string.Empty;
    public string YamlPath { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private readonly List<Build> _builds = [];
    public IReadOnlyCollection<Build> Builds => _builds.AsReadOnly();

    private Pipeline() { }

    public static Pipeline Create(string name, string repo, string branch = "main", string yamlPath = "") { }

    public Build TriggerBuild(string triggerEvent, string commitSha, int priority = 0) { }

    public void UpdateConfig(string branch, string yamlPath) { }
}
