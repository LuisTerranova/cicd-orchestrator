using Microsoft.Extensions.Logging;
using Orchestrator.Runner.WebSocket;

namespace Orchestrator.Runner.Monitoring;

public sealed class ProgressReporter
{
    private readonly ServerWebSocketClient _ws;
    private readonly ILogger<ProgressReporter> _logger;

    public ProgressReporter(ServerWebSocketClient ws, ILogger<ProgressReporter> logger)
    {
        _ws = ws;
        _logger = logger;
    }

    public async Task ReportProgressAsync(Guid jobId, string phase, int percent)
    {
        try
        {
            await _ws.SendMessageAsync($"progress:{jobId}:{phase}:{percent}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to send progress for job {JobId}: {Ex}", jobId, ex.Message);
        }
    }
}
