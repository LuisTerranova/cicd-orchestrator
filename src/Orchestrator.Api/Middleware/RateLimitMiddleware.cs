using System.Collections.Concurrent;

namespace Orchestrator.Api.Middleware;

public sealed class RateLimitMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly ConcurrentDictionary<string, RateLimitState> _clients = new();

    private static readonly RateLimitRule[] Rules =
    [
        new("/api/webhooks", 100, TimeSpan.FromMinutes(1)),
        new("/api/builds", 30, TimeSpan.FromMinutes(1)),
        new("/", 200, TimeSpan.FromMinutes(1)),
    ];

    private static readonly Timer _cleanupTimer = new(
        _ => CleanupStaleEntries(),
        null,
        TimeSpan.FromMinutes(1),
        TimeSpan.FromMinutes(1)
    );

    public RateLimitMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var path = context.Request.Path.Value ?? "/";
        var rule = GetMatchingRule(path);

        var key = $"{ip}:{rule.Prefix}";
        var now = DateTime.UtcNow;

        var state = _clients.GetOrAdd(key, _ => new RateLimitState());
        lock (state)
        {
            state.Entries.RemoveAll(e => now - e > rule.Window);
            state.Entries.Add(now);

            var remaining = rule.Limit - state.Entries.Count;

            context.Response.Headers["X-RateLimit-Limit"] = rule.Limit.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, remaining).ToString();
            context.Response.Headers["X-RateLimit-Reset"] = ((DateTimeOffset)now.Add(rule.Window)).ToUnixTimeSeconds().ToString();

            if (state.Entries.Count >= rule.Limit)
            {
                context.Response.StatusCode = 429;
                context.Response.Headers["Retry-After"] = rule.Window.TotalSeconds.ToString("F0");
                return;
            }
        }

        await _next(context);
    }

    private static void CleanupStaleEntries()
    {
        var cutoff = DateTime.UtcNow - Rules.Max(r => r.Window);
        foreach (var key in _clients.Keys)
        {
            if (_clients.TryGetValue(key, out var state))
            {
                lock (state)
                {
                    state.Entries.RemoveAll(e => e < cutoff);
                    if (state.Entries.Count == 0)
                        _clients.TryRemove(key, out _);
                }
            }
        }
    }

    private static RateLimitRule GetMatchingRule(string path)
    {
        foreach (var rule in Rules)
        {
            if (path.StartsWith(rule.Prefix, StringComparison.OrdinalIgnoreCase))
                return rule;
        }
        return Rules[^1];
    }

    private sealed class RateLimitState
    {
        public List<DateTime> Entries { get; } = new();
    }

    private sealed record RateLimitRule(string Prefix, int Limit, TimeSpan Window);
}
