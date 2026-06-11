using Microsoft.Extensions.Logging;
using Moq;
using Orchestrator.Application.Builds;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Domain.ValueObjects;

namespace Orchestrator.Tests.Application;

public sealed class CancelBuildHandlerTests
{
    private readonly Mock<IBuildRepository> _builds = new();
    private readonly Mock<IJobRepository> _jobs = new();
    private readonly Mock<IDomainEventDispatcher> _events = new();
    private readonly Mock<ILogger<CancelBuildHandler>> _logger = new();
    private readonly CancelBuildHandler _sut;

    public CancelBuildHandlerTests()
    {
        _sut = new CancelBuildHandler(_builds.Object, _jobs.Object, _events.Object, _logger.Object);
    }

    [Fact]
    public async Task HandleAsync_QueuedBuild_CancelsSuccessfully()
    {
        var pipelineId = Guid.NewGuid();
        var build = Build.Create(pipelineId, "push", "abc123");
        _builds.Setup(b => b.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(build);

        await _sut.HandleAsync(new CancelBuildCommand(build.Id));

        Assert.Equal(BuildStatus.Cancelled, build.Status);
        _builds.Verify(b => b.UpdateAsync(build, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AlreadyPassedBuild_DoesNotCancel()
    {
        var pipelineId = Guid.NewGuid();
        var build = Build.Create(pipelineId, "push", "abc123");
        build.Start();
        build.Complete(BuildStatus.Passed);

        _builds.Setup(b => b.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(build);

        await _sut.HandleAsync(new CancelBuildCommand(build.Id));

        Assert.Equal(BuildStatus.Passed, build.Status);
        _builds.Verify(b => b.UpdateAsync(It.IsAny<Build>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_BuildNotFound_Throws()
    {
        _builds.Setup(b => b.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Build?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.HandleAsync(new CancelBuildCommand(Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_CancelsPendingJobs()
    {
        var pipelineId = Guid.NewGuid();
        var build = Build.Create(pipelineId, "push", "abc123");
        var job1 = Job.Create(build.Id, "build");
        var job2 = Job.Create(build.Id, "test");
        build.AddJobs([job1, job2]);
        build.Start();

        _builds.Setup(b => b.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(build);

        await _sut.HandleAsync(new CancelBuildCommand(build.Id));

        Assert.Equal(JobStatus.Cancelled, job1.Status);
        Assert.Equal(JobStatus.Cancelled, job2.Status);
    }
}
