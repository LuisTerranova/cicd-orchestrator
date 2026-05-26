using Orchestrator.Domain.Events;
using Orchestrator.Domain.Exceptions;
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
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Runner name cannot be empty.");

        return new Runner
        {
            Id = Guid.NewGuid(),
            Name = name,
            Labels = labels ?? [],
            Os = os ?? string.Empty,
            Arch = arch ?? string.Empty,
            Status = RunnerStatus.Offline,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Register()
    {
        Status = RunnerStatus.Idle;
        LastSeen = DateTime.UtcNow;
        AddDomainEvent(new RunnerRegisteredEvent(Id, Name));
    }

    public void GoBusy()
    {
        if (Status != RunnerStatus.Idle)
            throw new DomainException("Only idle runners can go busy.");

        var oldStatus = Status;
        Status = RunnerStatus.Busy;
        AddDomainEvent(new RunnerStatusChangedEvent(Id, oldStatus, Status));
    }

    public void GoIdle()
    {
        if (Status != RunnerStatus.Busy)
            throw new DomainException("Only busy runners can go idle.");

        var oldStatus = Status;
        Status = RunnerStatus.Idle;
        LastSeen = DateTime.UtcNow;
        AddDomainEvent(new RunnerStatusChangedEvent(Id, oldStatus, Status));
    }

    public void Disconnect()
    {
        var oldStatus = Status;
        Status = RunnerStatus.Disconnected;
        AddDomainEvent(new RunnerStatusChangedEvent(Id, oldStatus, Status));
    }

    public void Revoke()
    {
        var oldStatus = Status;
        Status = RunnerStatus.Revoked;
        AddDomainEvent(new RunnerStatusChangedEvent(Id, oldStatus, Status));
    }

    public void Heartbeat()
    {
        LastSeen = DateTime.UtcNow;
    }

    public bool HasLabel(string requiredLabel)
    {
        if (string.IsNullOrWhiteSpace(requiredLabel))
            return false;

        return Labels.Contains(requiredLabel, StringComparer.OrdinalIgnoreCase);
    }
}
