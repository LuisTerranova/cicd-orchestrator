using Orchestrator.Api.Decorators;
using Orchestrator.Application.Common;
using Orchestrator.Infrastructure.Persistence;

namespace Orchestrator.Api.Extensions;

public static class CommandHandlerExtensions
{
    public static IServiceCollection AddDecoratedCommandHandler<TCommand, THandler>(
        this IServiceCollection services
    )
        where THandler : class, ICommandHandler<TCommand>
    {
        services.AddScoped<THandler>();
        services.AddScoped<ICommandHandler<TCommand>>(sp =>
        {
            var handler = sp.GetRequiredService<THandler>();
            var dbContext = sp.GetRequiredService<OrchestratorDbContext>();
            var logger = sp.GetRequiredService<ILogger<LoggingCommandHandlerDecorator<TCommand>>>();
            var uow = new UnitOfWorkCommandHandlerDecorator<TCommand>(handler, dbContext);
            return new LoggingCommandHandlerDecorator<TCommand>(uow, logger);
        });
        return services;
    }

    public static IServiceCollection AddDecoratedCommandHandler<TCommand, TResult, THandler>(
        this IServiceCollection services
    )
        where THandler : class, ICommandHandler<TCommand, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<ICommandHandler<TCommand, TResult>>(sp =>
        {
            var handler = sp.GetRequiredService<THandler>();
            var dbContext = sp.GetRequiredService<OrchestratorDbContext>();
            var logger = sp.GetRequiredService<ILogger<LoggingCommandHandlerDecorator<TCommand, TResult>>>();
            var uow = new UnitOfWorkCommandHandlerDecorator<TCommand, TResult>(handler, dbContext);
            return new LoggingCommandHandlerDecorator<TCommand, TResult>(uow, logger);
        });
        return services;
    }

    public static IServiceCollection AddLoggingCommandHandler<TCommand, THandler>(
        this IServiceCollection services
    )
        where THandler : class, ICommandHandler<TCommand>
    {
        services.AddScoped<THandler>();
        services.AddScoped<ICommandHandler<TCommand>>(sp =>
        {
            var concreteHandler = sp.GetRequiredService<THandler>();
            var logger = sp.GetRequiredService<ILogger<LoggingCommandHandlerDecorator<TCommand>>>();
            return new LoggingCommandHandlerDecorator<TCommand>(concreteHandler, logger);
        });
        return services;
    }
}
