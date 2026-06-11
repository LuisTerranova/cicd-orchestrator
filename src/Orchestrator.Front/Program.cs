using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Orchestrator.Front;
using Orchestrator.Front.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration.GetValue<string>("ApiBaseUrl") ?? "http://localhost:5000";

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
});

builder.Services.AddScoped(sp =>
{
    var client = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
    return new OrchestratorApiClient(client);
});

builder.Services.AddScoped<AuthService>();

await builder.Build().RunAsync();
