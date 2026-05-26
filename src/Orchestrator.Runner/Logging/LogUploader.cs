using System.Net.Http.Json;
using System.Text;

namespace Orchestrator.Runner.Logging;

public sealed class LogUploader
{
    private readonly HttpClient _http;

    public LogUploader(HttpClient httpClient)
    {
        _http = httpClient;
    }

    // Uploads a log chunk with Content-Range header for resumable uploads.
    // Uses a marker file on disk to track byte offset between chunks,
    // so interrupted uploads can be resumed from the last committed position.
    public async Task UploadChunkAsync(
        Guid jobId,
        string content,
        int lineCount,
        CancellationToken ct
    )
    {
        var markerPath = GetMarkerPath(jobId);
        var offset = 0L;
        if (File.Exists(markerPath))
        {
            var raw = await File.ReadAllTextAsync(markerPath, ct);
            offset = long.Parse(raw);
        }

        var bytes = Encoding.UTF8.GetBytes(content);
        var request = new HttpRequestMessage(HttpMethod.Post, $"api/jobs/{jobId}/logs")
        {
            Content = new ByteArrayContent(bytes),
        };

        // Inform the server where this chunk fits in the overall log stream.
        request.Headers.Add("Content-Range", $"bytes {offset}-{offset + bytes.Length - 1}/*");

        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        // Advance the marker to reflect bytes committed on the server.
        await File.WriteAllTextAsync(markerPath, (offset + bytes.Length).ToString(), ct);
    }

    // Signals the server that all log data has been sent, then removes the marker file.
    public async Task FinalizeAsync(Guid jobId, int totalLines, CancellationToken ct)
    {
        var markerPath = GetMarkerPath(jobId);
        if (File.Exists(markerPath))
        {
            File.Delete(markerPath);
        }

        var response = await _http.PostAsJsonAsync(
            $"api/jobs/{jobId}/logs/finalize",
            new { totalLines },
            ct
        );
        response.EnsureSuccessStatusCode();
    }

    private static string GetMarkerPath(Guid jobId)
    {
        return Path.Combine(Path.GetTempPath(), $"runner-log-{jobId:N}.marker");
    }
}
