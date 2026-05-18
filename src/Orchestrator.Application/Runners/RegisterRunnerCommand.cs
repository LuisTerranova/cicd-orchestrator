namespace Orchestrator.Application.Runners;

public sealed record RegisterRunnerCommand(string Name, string[] Labels, string Os, string Arch);
