using Orchestrator.Api.Extensions;
using Orchestrator.Application;
using Orchestrator.Application.Builds;
using Orchestrator.Application.Common;
using Orchestrator.Application.Jobs;
using Orchestrator.Application.Logs;
using Orchestrator.Application.Pipelines;
using Orchestrator.Application.Runners;
using Orchestrator.Application.Webhooks;
using Orchestrator.Infrastructure;
using Orchestrator.Infrastructure.Persistence;

DotNetEnv.Env.Load();

var connStr = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
if (!string.IsNullOrEmpty(connStr))
{
    var formattedConnStr = DbConnectionHelper.FormatConnectionString(connStr);
    Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", formattedConnStr);
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
builder.Services.AddLoggingCommandHandler<ProcessWebhookCommand, ProcessWebhookHandler>();

builder.Services.AddEndpoints();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapEndpoints();

app.Run();
