namespace Orchestrator.Domain.Entities;

public class Pipeline : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Repo { get; private set; } = string.Empty;
    public string Branch { get; private set; } = string.Empty;
    public string YamlPath { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private readonly List<Build> _builds = [];
    public IReadOnlyCollection<Build> Builds => _builds.AsReadOnly();

    private Pipeline() { }

    public static Pipeline Create(string name, string repo, string branch = "main", string yamlPath = "")
        => throw new NotImplementedException();

    public Build TriggerBuild(string triggerEvent, string commitSha, int priority = 0)
        => throw new NotImplementedException();

    public void UpdateConfig(string branch, string yamlPath)
        => throw new NotImplementedException();
}
