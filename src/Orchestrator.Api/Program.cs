using Orchestrator.Api.Endpoints;
using Orchestrator.Api.Extensions;
using Orchestrator.Application;
using Orchestrator.Application.Common;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Infrastructure;
using Orchestrator.Infrastructure.Persistence;

DotNetEnv.Env.Load();

var connStr = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
if (!string.IsNullOrEmpty(connStr))
{
    var formattedConnStr = Orchestrator.Infrastructure.Persistence.DbConnectionHelper.FormatConnectionString(connStr);
    Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", formattedConnStr);
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<OrchestratorDbContext>());
builder.Services.AddOpenApi();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPipelinesEndpoints();
app.MapBuildsEndpoints();
app.MapRunnersEndpoints();
app.MapJobsEndpoints();
app.MapWebhooksEndpoints();
app.MapLogsEndpoints();
app.MapHealthEndpoints();

app.Run();
