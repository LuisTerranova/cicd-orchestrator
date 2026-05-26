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
            var concreteHandler = sp.GetRequiredService<THandler>();
            var dbContext = sp.GetRequiredService<OrchestratorDbContext>();
            var logger = sp.GetRequiredService<ILogger<LoggingCommandHandlerDecorator<TCommand>>>();

            var uowDecorator = new UnitOfWorkCommandHandlerDecorator<TCommand>(
                concreteHandler,
                dbContext
            );
            var loggingDecorator = new LoggingCommandHandlerDecorator<TCommand>(
                uowDecorator,
                logger
            );

            return loggingDecorator;
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
            var concreteHandler = sp.GetRequiredService<THandler>();
            var dbContext = sp.GetRequiredService<OrchestratorDbContext>();
            var logger = sp.GetRequiredService<
                ILogger<LoggingCommandHandlerDecorator<TCommand, TResult>>
            >();

            var uowDecorator = new UnitOfWorkCommandHandlerDecorator<TCommand, TResult>(
                concreteHandler,
                dbContext
            );
            var loggingDecorator = new LoggingCommandHandlerDecorator<TCommand, TResult>(
                uowDecorator,
                logger
            );

            return loggingDecorator;
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
