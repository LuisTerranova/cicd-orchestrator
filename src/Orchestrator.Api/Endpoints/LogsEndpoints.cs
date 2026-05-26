using Orchestrator.Api.Extensions;
using Orchestrator.Application.Logs;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Infrastructure.Persistence;

namespace Orchestrator.Api.Endpoints;

public class LogsEndpoints : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/builds/logs").WithTags("Logs");

        group.MapGet("/{jobId:guid}", GetLogByJobIdAsync);
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

    private static async Task<IResult> UploadLogChunkAsync(
        Guid jobId,
        HttpRequest request,
        ILogRepository logRepository,
        OrchestratorDbContext dbContext,
        CancellationToken ct
    )
    {
        // Read the incoming log text chunk from the request body
        using var reader = new StreamReader(request.Body);
        var content = await reader.ReadToEndAsync(ct);

        // Define target directory and file name based on the Job ID
        var filePath = Path.Combine("/tmp/logs", $"{jobId}.log");
        var directory = Path.GetDirectoryName(filePath);
        if (directory != null)
        {
            Directory.CreateDirectory(directory);
        }

        // Append the received chunk to the log file on disk
        await File.AppendAllTextAsync(filePath, content, ct);

        // Re-calculate the current line count and file size
        var lineCount = File.Exists(filePath) ? File.ReadLines(filePath).Count() : 0;
        var sizeBytes = File.Exists(filePath) ? new FileInfo(filePath).Length : 0L;

        // Upsert the log metadata in the database to keep it in sync with disk
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
        // Retrieve log metadata to finalize its stats
        var logMetadata = await logRepository.GetByJobIdAsync(jobId, ct);
        if (logMetadata == null)
            return Results.NotFound();

        var filePath = logMetadata.FilePath;
        var sizeBytes = File.Exists(filePath)
            ? new FileInfo(filePath).Length
            : logMetadata.SizeBytes;

        // Use provided final line count or fall back to current metadata count
        var finalLines = lines ?? totalLines ?? logMetadata.LineCount;

        logMetadata.Update(finalLines, sizeBytes);
        await logRepository.UpdateAsync(logMetadata, ct);
        await dbContext.SaveChangesAsync(ct);

        return Results.Ok();
    }
}
