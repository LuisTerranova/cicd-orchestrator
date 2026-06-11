using System.Net.Http.Headers;
using Orchestrator.Runner.Registration;

namespace Orchestrator.Runner.Artifacts;

public sealed class ArtifactUploader
{
    private readonly HttpClient _http;
    private readonly CredentialStore _credentials;

    public ArtifactUploader(HttpClient httpClient, CredentialStore credentials)
    {
        _http = httpClient;
        _credentials = credentials;
    }

    // Uploads a file as multipart/form-data to the build's artifact endpoint.
    // The file stream is sent with application/octet-stream content type
    // and the original filename is included in the multipart disposition.
    // Throws if filePath is outside workspacePath (path traversal guard).
    public async Task UploadAsync(Guid buildId, string filePath, string workspacePath, CancellationToken ct)
    {
        var fullPath = Path.GetFullPath(filePath);
        var workspaceFull = Path.GetFullPath(workspacePath);
        if (!fullPath.StartsWith(workspaceFull, StringComparison.Ordinal))
            throw new InvalidOperationException($"Artifact path {filePath} is outside workspace.");

        using var fileStream = File.OpenRead(fullPath);
        using var fileContent = new StreamContent(fileStream);

        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        using var form = new MultipartFormDataContent();
        form.Add(fileContent, "file", Path.GetFileName(filePath));

        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/builds/{buildId}/artifacts")
        {
            Content = form
        };

        var creds = await _credentials.LoadAsync();
        if (creds?.Secret != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", creds.Value.Secret);
        }

        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }
}
