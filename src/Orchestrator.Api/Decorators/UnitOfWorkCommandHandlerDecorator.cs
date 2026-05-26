using Orchestrator.Application.Common;
using Orchestrator.Infrastructure.Persistence;

namespace Orchestrator.Api.Decorators;

public sealed class UnitOfWorkCommandHandlerDecorator<TCommand>(
    ICommandHandler<TCommand> decoratee,
    OrchestratorDbContext dbContext
) : ICommandHandler<TCommand>
{
    public async Task HandleAsync(TCommand command, CancellationToken ct = default)
    {
        await decoratee.HandleAsync(command, ct);
        await dbContext.SaveChangesAsync(ct);
    }
}

public sealed class UnitOfWorkCommandHandlerDecorator<TCommand, TResult>(
    ICommandHandler<TCommand, TResult> decoratee,
    OrchestratorDbContext dbContext
) : ICommandHandler<TCommand, TResult>
{
    public async Task<TResult> HandleAsync(TCommand command, CancellationToken ct = default)
    {
        var result = await decoratee.HandleAsync(command, ct);
        await dbContext.SaveChangesAsync(ct);
        return result;
    }
}
