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
        => throw new NotImplementedException();
}