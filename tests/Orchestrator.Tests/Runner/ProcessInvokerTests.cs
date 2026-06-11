using Orchestrator.Runner.Execution;

namespace Orchestrator.Tests.Runner;

public sealed class ProcessInvokerTests
{
    private readonly ProcessInvoker _sut = new();

    [Fact]
    public async Task RunAsync_SuccessfulCommand_ReturnsExitCodeZero()
    {
        var result = await _sut.RunAsync("echo", ["hello"], CancellationToken.None);
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("hello", result.Stdout);
    }

    [Fact]
    public async Task RunAsync_FailingCommand_ReturnsNonZeroExitCode()
    {
        var result = await _sut.RunAsync("sh", ["-c", "exit 42"], CancellationToken.None);
        Assert.Equal(42, result.ExitCode);
    }

    [Fact]
    public async Task RunAsync_CancellationToken_KillsProcess()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _sut.RunAsync("sleep", ["10"], cts.Token));
    }

    [Fact]
    public async Task RunAsync_CommandNotFound_Throws()
    {
        await Assert.ThrowsAsync<System.ComponentModel.Win32Exception>(() =>
            _sut.RunAsync("nonexistent-command-xyz", [], CancellationToken.None));
    }
}
