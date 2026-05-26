using Orchestrator.Application.Common;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Runners;

public sealed class RegisterRunnerHandler(
    IRunnerRepository runners,
    IDomainEventDispatcher eventDispatcher
) : ICommandHandler<RegisterRunnerCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        RegisterRunnerCommand command,
        CancellationToken ct = default
    )
    {
        var runner = Domain.Entities.Runner.Create(
            command.Name,
            command.Labels,
            command.Os,
            command.Arch
        );
        runner.Register();
        await runners.AddAsync(runner, ct);
        await eventDispatcher.DispatchAsync(runner.DomainEvents, ct);
        runner.ClearDomainEvents();
        return runner.Id;
    }
}
