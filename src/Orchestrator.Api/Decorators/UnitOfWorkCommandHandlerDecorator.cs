using Microsoft.EntityFrameworkCore;
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
        await using var tx = await dbContext.Database.BeginTransactionAsync(ct);
        try
        {
            await decoratee.HandleAsync(command, ct);
            await dbContext.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}

public sealed class UnitOfWorkCommandHandlerDecorator<TCommand, TResult>(
    ICommandHandler<TCommand, TResult> decoratee,
    OrchestratorDbContext dbContext
) : ICommandHandler<TCommand, TResult>
{
    public async Task<TResult> HandleAsync(TCommand command, CancellationToken ct = default)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync(ct);
        try
        {
            var result = await decoratee.HandleAsync(command, ct);
            await dbContext.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return result;
        }
        catch
        {
            await tx.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
