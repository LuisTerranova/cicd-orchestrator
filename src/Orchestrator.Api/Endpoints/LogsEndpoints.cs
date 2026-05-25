using Orchestrator.Api.Extensions;
using Orchestrator.Application.Logs;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Api.Endpoints;

public static class LogsEndpoints
{
    public static void MapLogsEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /api/builds/logs/{jobId:guid} - Retrieve log metadata by job ID
        app.MapGet("/api/builds/logs/{jobId:guid}", async (Guid jobId, HttpContext http, CancellationToken ct) =>
        {
            var query = http.RequestServices.GetRequiredService<GetLogByJobIdQuery>();
            var log = await query.HandleAsync(jobId, ct);
            if (log == null)
                return Results.NotFound();

            var response = new LogResponse(log.Id, log.JobId, log.FilePath, log.LineCount, log.SizeBytes, log.CreatedAt);
            return Results.Ok(response);
        });

        // POST /api/builds/logs/{jobId:guid} - Upload a chunk of job log content
        app.MapPost("/api/builds/logs/{jobId:guid}", async (Guid jobId, HttpRequest request, HttpContext http, CancellationToken ct) =>
        {
            using var reader = new StreamReader(request.Body);
            var content = await reader.ReadToEndAsync(ct);

            var filePath = Path.Combine("/tmp/logs", $"{jobId}.log");
            var directory = Path.GetDirectoryName(filePath);
            if (directory != null)
            {
                Directory.CreateDirectory(directory);
            }

            // Append chunk in append mode
            await File.AppendAllTextAsync(filePath, content, ct);

            // Re-read file metrics
            var lineCount = File.Exists(filePath) ? File.ReadLines(filePath).Count() : 0;
            var sizeBytes = File.Exists(filePath) ? new FileInfo(filePath).Length : 0L;

            var logRepository = http.RequestServices.GetRequiredService<ILogRepository>();
            var unitOfWork = http.RequestServices.GetRequiredService<IUnitOfWork>();

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

            await unitOfWork.SaveChangesAsync(ct);
            return Results.Ok();
        });

        // POST /api/builds/logs/{jobId:guid}/finalize - Finalize log metadata for a completed job
        app.MapPost("/api/builds/logs/{jobId:guid}/finalize", async (Guid jobId, int? lines, int? totalLines, HttpContext http, CancellationToken ct) =>
        {
            var logRepository = http.RequestServices.GetRequiredService<ILogRepository>();
            var unitOfWork = http.RequestServices.GetRequiredService<IUnitOfWork>();

            var logMetadata = await logRepository.GetByJobIdAsync(jobId, ct);
            if (logMetadata == null)
                return Results.NotFound();

            var filePath = logMetadata.FilePath;
            var sizeBytes = File.Exists(filePath) ? new FileInfo(filePath).Length : logMetadata.SizeBytes;
            var finalLines = lines ?? totalLines ?? logMetadata.LineCount;

            logMetadata.Update(finalLines, sizeBytes);
            await logRepository.UpdateAsync(logMetadata, ct);
            await unitOfWork.SaveChangesAsync(ct);

            return Results.Ok();
        });
    }
}

