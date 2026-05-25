using System.Net.Http.Headers;

namespace Orchestrator.Runner.Artifacts;

public sealed class ArtifactUploader
{
    private readonly HttpClient _http;

    public ArtifactUploader(HttpClient httpClient)
    {
        _http = httpClient;
    }

    // Uploads a file as multipart/form-data to the build's artifact endpoint.
    // The file stream is sent with application/octet-stream content type
    // and the original filename is included in the multipart disposition.
    public async Task UploadAsync(Guid buildId, string filePath, CancellationToken ct)
    {
        using var fileStream = File.OpenRead(filePath);
        using var fileContent = new StreamContent(fileStream);

        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        using var form = new MultipartFormDataContent();
        form.Add(fileContent, "file", Path.GetFileName(filePath));

        var response = await _http.PostAsync($"api/builds/{buildId}/artifacts", form, ct);
        response.EnsureSuccessStatusCode();
    }
}
