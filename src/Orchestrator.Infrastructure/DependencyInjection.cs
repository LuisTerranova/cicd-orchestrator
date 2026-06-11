using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Infrastructure.Auth;
using Orchestrator.Infrastructure.Configuration;
using Orchestrator.Infrastructure.Persistence;
using Orchestrator.Infrastructure.Persistence.Repositories;
using Orchestrator.Infrastructure.Services;
using Orchestrator.Infrastructure.Webhooks;
using MassTransit;

namespace Orchestrator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<OrchestratorDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
        );

        services.AddScoped<IBuildRepository, EfBuildRepository>();
        services.AddScoped<IJobRepository, EfJobRepository>();
        services.AddScoped<IRunnerRepository, EfRunnerRepository>();
        services.AddScoped<IPipelineRepository, EfPipelineRepository>();
        services.AddScoped<IArtifactRepository, EfArtifactRepository>();
        services.AddScoped<ILogRepository, EfLogRepository>();

        services.AddSingleton<IRunnerTokenGenerator, JwtRunnerTokenGenerator>();
        services.AddSingleton<IWebhookSignatureValidator, HmacWebhookSignatureValidator>();
        services.AddHttpClient<IWebhookDispatcher, HttpClientWebhookDispatcher>();

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.Configure<RunnerExpiryOptions>(configuration.GetSection("RunnerExpiry"));

        services.AddMassTransit(bus =>
        {
            bus.AddConsumer<JobCompletedConsumer>();

            bus.UsingRabbitMq((ctx, cfg) =>
            {
                var host = configuration["RabbitMq:Host"] ?? "localhost";
                var user = configuration["RabbitMq:Username"] ?? "guest";
                var pass = configuration["RabbitMq:Password"] ?? "guest";

                cfg.Host(new Uri($"rabbitmq://{host}:5672"), h =>
                {
                    h.Username(user);
                    h.Password(pass);
                });

                cfg.ReceiveEndpoint("server-jobs-completed", e =>
                {
                    e.ConfigureConsumer<JobCompletedConsumer>(ctx);
                });
            });
        });

        services.AddHostedService<JobDispatcherBackgroundService>();
        services.AddHostedService<LogRetentionService>();
        services.AddHostedService<ArtifactCleanupService>();
        services.AddHostedService<RunnerExpiryService>();

        return services;
    }
}
