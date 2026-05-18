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
        IDomainEventDispatcher eventDispatcher) { }

    public Task<Guid> HandleAsync(RegisterRunnerCommand command, CancellationToken ct = default)
        => throw new NotImplementedException();
}
