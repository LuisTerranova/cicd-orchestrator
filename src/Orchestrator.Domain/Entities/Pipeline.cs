using Orchestrator.Domain.Exceptions;

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
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Pipeline name cannot be empty.");
        if (string.IsNullOrWhiteSpace(repo))
            throw new DomainException("Repo cannot be empty.");

        return new Pipeline
        {
            Id = Guid.NewGuid(),
            Name = name,
            Repo = repo,
            Branch = branch,
            YamlPath = yamlPath ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };
    }

    public Build TriggerBuild(string triggerEvent, string commitSha, int priority = 0)
    {
        var build = Build.Create(Id, triggerEvent, commitSha, priority);
        _builds.Add(build);
        return build;
    }

    public void UpdateConfig(string branch, string yamlPath)
    {
        Branch = branch;
        YamlPath = yamlPath;
    }
}
