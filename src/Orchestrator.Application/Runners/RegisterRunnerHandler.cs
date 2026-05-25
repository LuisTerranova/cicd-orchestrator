using Orchestrator.Application.Common;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Runners;

public sealed class RegisterRunnerHandler
{
    private readonly IRunnerRepository _runners;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public RegisterRunnerHandler(
        IRunnerRepository runners,
        IUnitOfWork unitOfWork,
        IDomainEventDispatcher eventDispatcher)
    {
        _runners = runners;
        _unitOfWork = unitOfWork;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<Guid> HandleAsync(RegisterRunnerCommand command, CancellationToken ct = default)
    {
        var runner = Domain.Entities.Runner.Create(command.Name, command.Labels, command.Os, command.Arch);
        runner.Register();
        await _runners.AddAsync(runner, ct);
        await _eventDispatcher.DispatchAsync(runner.DomainEvents, ct);
        runner.ClearDomainEvents();
        await _unitOfWork.SaveChangesAsync(ct);
        return runner.Id;
    }
}
