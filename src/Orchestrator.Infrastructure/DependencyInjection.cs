using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Infrastructure.Auth;
using Orchestrator.Infrastructure.Persistence;
using Orchestrator.Infrastructure.Persistence.Repositories;
using Orchestrator.Infrastructure.Services;
using Orchestrator.Infrastructure.Webhooks;

namespace Orchestrator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrchestratorDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IBuildRepository, EfBuildRepository>();
        services.AddScoped<IJobRepository, EfJobRepository>();
        services.AddScoped<IRunnerRepository, EfRunnerRepository>();
        services.AddScoped<IPipelineRepository, EfPipelineRepository>();
        services.AddScoped<IArtifactRepository, EfArtifactRepository>();
        services.AddScoped<ILogRepository, EfLogRepository>();

        services.AddSingleton<IRunnerTokenGenerator, JwtRunnerTokenGenerator>();
        services.AddSingleton<IWebhookSignatureValidator, HmacWebhookSignatureValidator>();
        services.AddSingleton<IWebhookDispatcher, HttpClientWebhookDispatcher>();

        return services;
    }
}
