namespace Orchestrator.Runner.Logging;

public sealed class LogUploader
{
    public LogUploader(HttpClient httpClient)
    {
        throw new NotImplementedException();
    }

    public async Task UploadChunkAsync(Guid jobId, string content, int lineCount, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task FinalizeAsync(Guid jobId, int totalLines, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
