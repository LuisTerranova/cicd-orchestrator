using Orchestrator.Api.Endpoints;
using Orchestrator.Application;
using Orchestrator.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
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
