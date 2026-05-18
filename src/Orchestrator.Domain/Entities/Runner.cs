using Orchestrator.Domain.Events;
using Orchestrator.Domain.ValueObjects;

namespace Orchestrator.Domain.Entities;

public class Runner : Entity
{
    public string Name { get; private set; } = string.Empty;
    public RunnerStatus Status { get; private set; }
    public string[] Labels { get; private set; } = [];
    public string Os { get; private set; } = string.Empty;
    public string Arch { get; private set; } = string.Empty;
    public DateTime LastSeen { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Runner() { }

    public static Runner Create(string name, string[] labels, string os, string arch)
        => throw new NotImplementedException();

    public void Register()
        => throw new NotImplementedException();

    public void GoBusy()
        => throw new NotImplementedException();

    public void GoIdle()
        => throw new NotImplementedException();

    public void Disconnect()
        => throw new NotImplementedException();

    public void Revoke()
        => throw new NotImplementedException();

    public void Heartbeat()
        => throw new NotImplementedException();

    public bool HasLabel(string requiredLabel)
        => throw new NotImplementedException();
}
