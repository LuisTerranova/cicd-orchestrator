using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Infrastructure.Auth;

public static class DependencyInjection
{
    public static IServiceCollection AddRunnerAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AuthOptions>(configuration.GetSection("Auth"));
        services.AddSingleton<IRunnerTokenGenerator, JwtRunnerTokenGenerator>();
        return services;
    }
}
