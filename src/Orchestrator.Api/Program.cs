using Microsoft.EntityFrameworkCore;
using Orchestrator.Api.Extensions;
using Orchestrator.Api.Middleware;
using Orchestrator.Application;
using Orchestrator.Application.Builds;
using Orchestrator.Application.Common;
using Orchestrator.Application.Jobs;
using Orchestrator.Application.Logs;
using Orchestrator.Application.Pipelines;
using Orchestrator.Application.Runners;
using Orchestrator.Application.Webhooks;
using Orchestrator.Infrastructure;
using Orchestrator.Infrastructure.Auth;
using Orchestrator.Infrastructure.Persistence;
using Orchestrator.Infrastructure.PipelineEngine;

// Carrega .env do diretório corrente ou da raiz do repo
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
var envDir = Directory.GetCurrentDirectory();
while (!File.Exists(envPath) && envDir != null)
{
    envDir = Path.GetDirectoryName(envDir);
    if (envDir == null) break;
    envPath = Path.Combine(envDir, ".env");
}

if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
}

var connStr = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")?.Trim('"', '\'', ' ');
if (!string.IsNullOrEmpty(connStr))
{
    var formattedConnStr = DbConnectionHelper.FormatConnectionString(connStr);
    if (!string.IsNullOrEmpty(formattedConnStr))
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", formattedConnStr);
    }
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddDecoratedCommandHandler<TriggerBuildCommand, Guid, TriggerBuildHandler>();
builder.Services.AddDecoratedCommandHandler<AssignJobCommand, AssignJobHandler>();
builder.Services.AddDecoratedCommandHandler<CancelJobCommand, CancelJobHandler>();
builder.Services.AddDecoratedCommandHandler<CompleteJobCommand, CompleteJobHandler>();
builder.Services.AddDecoratedCommandHandler<UploadLogCommand, Guid, UploadLogHandler>();
builder.Services.AddDecoratedCommandHandler<CreatePipelineCommand, Guid, CreatePipelineHandler>();
builder.Services.AddDecoratedCommandHandler<RegisterRunnerCommand, Guid, RegisterRunnerHandler>();
builder.Services.AddDecoratedCommandHandler<CancelBuildCommand, CancelBuildHandler>();
builder.Services.AddDecoratedCommandHandler<UpdatePipelineYamlCommand, UpdatePipelineYamlHandler>();
builder.Services.AddDecoratedCommandHandler<UpdatePipelineCommand, UpdatePipelineHandler>();
builder.Services.AddDecoratedCommandHandler<DeletePipelineCommand, DeletePipelineHandler>();
builder.Services.AddLoggingCommandHandler<ProcessWebhookCommand, ProcessWebhookHandler>();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5091", "https://localhost:7071")
              .AllowAnyHeader()
              .AllowAnyMethod()
    )
);

builder.Services.AddPipelineEngine();
builder.Services.AddRunnerAuth(builder.Configuration);
builder.Services.AddObservability(builder.Configuration);
builder.Services.AddEndpoints();
builder.Services.AddOpenApi();

var app = builder.Build();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrchestratorDbContext>();
    try
    {
        await db.Database.MigrateAsync();

        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        if (env.IsDevelopment() || !await db.Users.AnyAsync())
        {
            await Orchestrator.Infrastructure.Persistence.DatabaseSeeder.SeedAsync(db);
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Database migration failed — continuing with existing schema");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseMiddleware<RateLimitMiddleware>();
app.UseMiddleware<RunnerAuthMiddleware>();

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.MapEndpoints();

app.Run();
