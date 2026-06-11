using System.Text.Json;
using Orchestrator.Api.Extensions;
using Orchestrator.Application.Common;
using Orchestrator.Application.Webhooks;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Api.Endpoints;

public class WebhooksEndpoints : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/v1/webhooks",
            async (
                HttpRequest httpRequest,
                ICommandHandler<ProcessWebhookCommand> processWebhookHandler,
                IPipelineTriggerMatcher triggerMatcher,
                IPipelineRepository pipelines,
                IPipelineYamlParser yamlParser,
                IWebhookSignatureValidator signatureValidator,
                Microsoft.Extensions.Configuration.IConfiguration configuration,
                ILogger<WebhooksEndpoints> logger,
                CancellationToken ct
            ) =>
            {
                using var reader = new StreamReader(httpRequest.Body);
                var payload = await reader.ReadToEndAsync(ct);

                var signature = httpRequest.Headers["X-Hub-Signature-256"].FirstOrDefault()
                    ?? httpRequest.Headers["X-Hub-Signature"].FirstOrDefault()
                    ?? "";

                var (branch, eventType, commitSha, actor, repoName) = ParsePayload(payload);

                var allPipelines = await pipelines.GetAllAsync(ct);
                var matched = false;

                foreach (var pipeline in allPipelines)
                {
                    if (string.IsNullOrWhiteSpace(pipeline.YamlContent))
                        continue;

                    if (!string.IsNullOrEmpty(repoName)
                        && !pipeline.Repo.EndsWith(repoName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    TriggerConfig? trigger = null;
                    try
                    {
                        var def = yamlParser.Parse(pipeline.YamlContent);
                        trigger = def.Trigger;
                    }
                    catch { }

                    if (!triggerMatcher.Matches(trigger, branch, eventType))
                        continue;

                    matched = true;

                    var webhookSecret = configuration["Auth:WebhookSecret"] ?? "dev-webhook-secret";

                    if (string.IsNullOrEmpty(signature))
                    {
                        logger.LogWarning("Missing webhook signature for pipeline {Pipeline}", pipeline.Name);
                        return Results.Unauthorized();
                    }

                    var cleanSignature = signature;
                    if (cleanSignature.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase))
                    {
                        cleanSignature = cleanSignature["sha256=".Length..];
                    }
                    else if (cleanSignature.StartsWith("sha1=", StringComparison.OrdinalIgnoreCase))
                    {
                        cleanSignature = cleanSignature["sha1=".Length..];
                    }

                    if (!signatureValidator.Validate(payload, cleanSignature, webhookSecret))
                    {
                        logger.LogWarning("Invalid webhook signature for pipeline {Pipeline}", pipeline.Name);
                        return Results.Unauthorized();
                    }

                    var command = new ProcessWebhookCommand(pipeline.Id, branch, eventType, commitSha, actor);
                    await processWebhookHandler.HandleAsync(command, ct);

                    logger.LogInformation(
                        "Webhook matched pipeline {Pipeline} for branch {Branch}/{Event}",
                        pipeline.Name, branch, eventType);

                    return Results.Ok(new { acknowledged = true, matched = true, branch, @event = eventType });
                }

                if (!matched)
                {
                    logger.LogInformation(
                        "Webhook acknowledged but no pipeline matched branch {Branch}/{Event}",
                        branch, eventType);
                }

                return Results.Ok(new { acknowledged = true, matched, branch, @event = eventType });
            }
        );
    }

    private static (string branch, string eventType, string commitSha, string actor, string? repoName) ParsePayload(string payload)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;

            var @ref = root.TryGetProperty("ref", out var refProp) ? refProp.GetString() ?? "" : "";
            var branch = @ref.Replace("refs/heads/", "");

            var eventType = root.TryGetProperty("object_kind", out var kindProp)
                ? kindProp.GetString() ?? "push"
                : "push";

            if (root.TryGetProperty("action", out var actionProp))
                eventType = actionProp.GetString() ?? eventType;

            var commitSha = root.TryGetProperty("after", out var afterProp) ? afterProp.GetString() ?? "" : "";
            if (string.IsNullOrEmpty(commitSha) && root.TryGetProperty("checkout_sha", out var checkoutProp))
                commitSha = checkoutProp.GetString() ?? "";
            if (string.IsNullOrEmpty(commitSha))
                commitSha = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

            var actor = "git";
            if (root.TryGetProperty("pusher", out var pusherProp) && pusherProp.TryGetProperty("name", out var pusherName))
                actor = pusherName.GetString() ?? "git";
            else if (root.TryGetProperty("sender", out var senderProp) && senderProp.TryGetProperty("login", out var senderLogin))
                actor = senderLogin.GetString() ?? "git";

            string? repoName = null;
            if (root.TryGetProperty("repository", out var repo))
            {
                if (repo.TryGetProperty("full_name", out var fullName))
                    repoName = fullName.GetString();
                else if (repo.TryGetProperty("name", out var name))
                    repoName = name.GetString();
            }

            return (branch, eventType, commitSha, actor, repoName);
        }
        catch
        {
            return ("unknown", "push", DateTime.UtcNow.ToString("yyyyMMddHHmmss"), "git", null);
        }
    }
}
