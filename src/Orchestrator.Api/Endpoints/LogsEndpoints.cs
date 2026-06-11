using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Orchestrator.Api.Extensions;
using Orchestrator.Application.Logs;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Infrastructure.Configuration;
using Orchestrator.Infrastructure.Persistence;

namespace Orchestrator.Api.Endpoints;

public class LogsEndpoints : IEndpoint
{
    private static readonly ConcurrentDictionary<Guid, int> _lineCounts = new();

    public static void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/builds/logs").WithTags("Logs");

        group.MapGet("/{jobId:guid}", GetLogByJobIdAsync);
        group.MapGet("/{jobId:guid}/content", GetLogContentAsync);
        group.MapPost("/{jobId:guid}", UploadLogChunkAsync);
        group.MapPost("/{jobId:guid}/finalize", FinalizeLogAsync);
    }

    private static async Task<IResult> GetLogByJobIdAsync(
        Guid jobId,
        GetLogByJobIdQuery query,
        CancellationToken ct
    )
    {
        var log = await query.HandleAsync(jobId, ct);
        if (log == null)
            return Results.NotFound();

        var response = new LogResponse(
            log.Id,
            log.JobId,
            log.FilePath,
            log.LineCount,
            log.SizeBytes,
            log.CreatedAt
        );
        return Results.Ok(response);
    }

    private static async Task<IResult> GetLogContentAsync(
        Guid jobId,
        int offset = 0,
        int limit = 100,
        ILogRepository logRepository = null!,
        IOptions<StorageOptions> storage = null!,
        CancellationToken ct = default
    )
    {
        var log = await logRepository.GetByJobIdAsync(jobId, ct);
        if (log is null || string.IsNullOrEmpty(log.FilePath) || !File.Exists(log.FilePath))
            return Results.Ok("");

        var lines = await File.ReadAllLinesAsync(log.FilePath, ct);
        var chunk = lines.Skip(offset).Take(limit).ToArray();
        return Results.Ok(string.Join("\n", chunk));
    }

    private static async Task<IResult> UploadLogChunkAsync(
        Guid jobId,
        HttpRequest request,
        ILogRepository logRepository,
        OrchestratorDbContext dbContext,
        IOptions<StorageOptions> storage,
        CancellationToken ct
    )
    {
        using var reader = new StreamReader(request.Body);
        var content = await reader.ReadToEndAsync(ct);

        var logDir = storage.Value.LogsPath;
        Directory.CreateDirectory(logDir);
        var filePath = Path.Combine(logDir, $"{jobId}.log");

        await File.AppendAllTextAsync(filePath, content, ct);

        var lineCount = _lineCounts.AddOrUpdate(
            jobId,
            _ => content.Count(c => c == '\n'),
            (_, existing) => existing + content.Count(c => c == '\n')
        );

        var fileInfo = new FileInfo(filePath);
        var sizeBytes = fileInfo.Exists ? fileInfo.Length : 0L;

        var logMetadata = await logRepository.GetByJobIdAsync(jobId, ct);
        if (logMetadata == null)
        {
            logMetadata = LogMetadata.Create(jobId, filePath, lineCount, sizeBytes);
            await logRepository.AddAsync(logMetadata, ct);
        }
        else
        {
            logMetadata.Update(lineCount, sizeBytes);
            await logRepository.UpdateAsync(logMetadata, ct);
        }

        await dbContext.SaveChangesAsync(ct);
        return Results.Ok();
    }

    private static async Task<IResult> FinalizeLogAsync(
        Guid jobId,
        int? lines,
        int? totalLines,
        ILogRepository logRepository,
        OrchestratorDbContext dbContext,
        CancellationToken ct
    )
    {
        var logMetadata = await logRepository.GetByJobIdAsync(jobId, ct);
        if (logMetadata == null)
            return Results.NotFound();

        var filePath = logMetadata.FilePath;
        var sizeBytes = File.Exists(filePath)
            ? new FileInfo(filePath).Length
            : logMetadata.SizeBytes;

        var finalLines = lines ?? totalLines ?? _lineCounts.GetOrAdd(jobId, logMetadata.LineCount);
        _lineCounts.TryRemove(jobId, out _);

        logMetadata.Update(finalLines, sizeBytes);
        await logRepository.UpdateAsync(logMetadata, ct);
        await dbContext.SaveChangesAsync(ct);

        return Results.Ok();
    }
}
