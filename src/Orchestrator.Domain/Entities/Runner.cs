using Orchestrator.Domain.Events;
using Orchestrator.Domain.Exceptions;
using Orchestrator.Domain.ValueObjects;

namespace Orchestrator.Domain.Entities;

public class Runner : Entity
{
    public required string Name { get; private set; }
    public RunnerStatus Status { get; private set; }
    public string[] Labels { get; private set; } = [];
    public string Os { get; private set; } = string.Empty;
    public string Arch { get; private set; } = string.Empty;
    public DateTime LastSeen { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Runner() { }

    public static Runner Create(string name, string[] labels, string os, string arch) { }

    public void Register() { }

    public void GoBusy() { }

    public void GoIdle() { }

    public void Disconnect() { }

    public void Revoke() { }

    public void Heartbeat() { }

    public bool HasLabel(string requiredLabel) { }
}
