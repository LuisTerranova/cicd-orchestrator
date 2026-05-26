using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Orchestrator.Application.Common;

public sealed class LoggingCommandHandlerDecorator<TCommand>(
    ICommandHandler<TCommand> decoratee,
    ILogger<LoggingCommandHandlerDecorator<TCommand>> logger
) : ICommandHandler<TCommand>
{
    public async Task HandleAsync(TCommand command, CancellationToken ct = default)
    {
        var commandName = typeof(TCommand).Name;
        logger.LogInformation("Handling command {CommandName}", commandName);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await decoratee.HandleAsync(command, ct);
            stopwatch.Stop();
            logger.LogInformation(
                "Command {CommandName} handled successfully in {ElapsedMs}ms",
                commandName,
                stopwatch.ElapsedMilliseconds
            );
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(
                ex,
                "Command {CommandName} failed: {ErrorMessage}",
                commandName,
                ex.Message
            );
            throw;
        }
    }
}

public sealed class LoggingCommandHandlerDecorator<TCommand, TResult>(
    ICommandHandler<TCommand, TResult> decoratee,
    ILogger<LoggingCommandHandlerDecorator<TCommand, TResult>> logger
) : ICommandHandler<TCommand, TResult>
{
    public async Task<TResult> HandleAsync(TCommand command, CancellationToken ct = default)
    {
        var commandName = typeof(TCommand).Name;
        logger.LogInformation("Handling command {CommandName}", commandName);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await decoratee.HandleAsync(command, ct);
            stopwatch.Stop();
            logger.LogInformation(
                "Command {CommandName} handled successfully in {ElapsedMs}ms",
                commandName,
                stopwatch.ElapsedMilliseconds
            );
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(
                ex,
                "Command {CommandName} failed: {ErrorMessage}",
                commandName,
                ex.Message
            );
            throw;
        }
    }
}
