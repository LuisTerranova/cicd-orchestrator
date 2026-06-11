using Moq;
using Moq.Protected;
using Orchestrator.Runner.Logging;

namespace Orchestrator.Tests.Runner;

public sealed class LogCapturerTests
{
    private readonly SecretMasker _masker = new();
    private readonly Mock<HttpMessageHandler> _httpHandler = new();
    private readonly LogCapturer _sut;

    public LogCapturerTests()
    {
        var httpClient = new HttpClient(_httpHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:5000"),
        };
        var uploader = new LogUploader(httpClient);
        _sut = new LogCapturer(_masker, uploader, 1024 * 1024);
    }

    [Fact]
    public void CaptureLine_WithTimestamp_StoresEntry()
    {
        var jobId = Guid.NewGuid();
        _sut.StartCapture(jobId, new());
        _sut.CaptureLine(jobId, "stdout", "hello world");
    }

    [Fact]
    public async Task FlushAsync_WithBufferedLines_UploadsChunk()
    {
        var jobId = Guid.NewGuid();
        _sut.StartCapture(jobId, new());
        _sut.CaptureLine(jobId, "stdout", "line1");

        _httpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage());

        await _sut.FlushAsync(jobId, CancellationToken.None);

        _httpHandler.Protected().Verify("SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task CompleteAsync_FlushesAndFinalizes()
    {
        var jobId = Guid.NewGuid();
        _sut.StartCapture(jobId, new());
        _sut.CaptureLine(jobId, "stdout", "test");

        _httpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage());

        await _sut.CompleteAsync(jobId, CancellationToken.None);

        _httpHandler.Protected().Verify("SendAsync",
            Times.AtLeastOnce(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task CompleteAsync_UnknownJob_DoesNothing()
    {
        await _sut.CompleteAsync(Guid.NewGuid(), CancellationToken.None);
    }
}
