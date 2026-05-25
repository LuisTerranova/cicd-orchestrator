using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace Orchestrator.Runner.Logging;

public sealed class LogCapturer : IDisposable
{
    private readonly SecretMasker _masker;
    private readonly LogUploader _uploader;
    private readonly long _maxSizeBytes;
    private readonly ConcurrentDictionary<Guid, JobLogState> _jobs = new();

    public LogCapturer(SecretMasker masker, LogUploader uploader, long maxSizeBytes)
    {
        _masker = masker;
        _uploader = uploader;
        _maxSizeBytes = maxSizeBytes;
    }

    public void StartCapture(Guid jobId, Dictionary<string, string> secrets)
    {
        _jobs[jobId] = new JobLogState { Secrets = secrets };
    }

    // Masks secrets in the line using SecretMasker, then appends a JSON Line entry
    // (timestamp, stream name, masked content). Auto-flushes when the buffer
    // reaches _maxSizeBytes to keep memory bounded.
    public void CaptureLine(Guid jobId, string stream, string line)
    {
        if (!_jobs.TryGetValue(jobId, out var state))
            return;

        var masked = _masker.Mask(line, state.Secrets);
        var entry = JsonSerializer.Serialize(new { ts = DateTime.UtcNow, stream, line = masked });
        state.Buffer.Add(entry);
        state.TotalLines++;
        state.BufferSize += Encoding.UTF8.GetByteCount(entry + "\n");

        if (state.BufferSize >= _maxSizeBytes)
        {
            _ = FlushAsync(jobId, CancellationToken.None);
        }
    }

    // Atomically swaps the buffer to avoid data loss while uploading in flight.
    public async Task FlushAsync(Guid jobId, CancellationToken ct)
    {
        if (!_jobs.TryGetValue(jobId, out var state))
            return;

        if (state.Buffer.Count == 0)
            return;

        var batch = Interlocked.Exchange(ref state.Buffer, new List<string>());
        var content = string.Join("\n", batch) + "\n";
        state.BufferSize = 0;

        await _uploader.UploadChunkAsync(jobId, content, batch.Count, ct);
    }

    // Flushes remaining lines and finalizes the upload on the server.
    public async Task CompleteAsync(Guid jobId, CancellationToken ct)
    {
        await FlushAsync(jobId, ct);

        if (_jobs.TryRemove(jobId, out var state))
        {
            await _uploader.FinalizeAsync(jobId, state.TotalLines, ct);
        }
    }

    public void Dispose()
    {
        foreach (var kvp in _jobs)
        {
            kvp.Value.Buffer.Clear();
        }

        _jobs.Clear();
    }

    private sealed class JobLogState
    {
        public List<string> Buffer = [];
        public int BufferSize;
        public int TotalLines;
        public Dictionary<string, string> Secrets = [];
    }
}
