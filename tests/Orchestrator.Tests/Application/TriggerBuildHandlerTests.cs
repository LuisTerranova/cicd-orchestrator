using Microsoft.Extensions.Logging;
using Moq;
using Orchestrator.Application.Builds;
using Orchestrator.Domain.Entities;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Domain.ValueObjects;

namespace Orchestrator.Tests.Application;

public sealed class TriggerBuildHandlerTests
{
    private readonly Mock<IPipelineRepository> _pipelines = new();
    private readonly Mock<IBuildRepository> _builds = new();
    private readonly Mock<IJobRepository> _jobs = new();
    private readonly Mock<IPipelineYamlParser> _yamlParser = new();
    private readonly Mock<IPipelineTriggerMatcher> _triggerMatcher = new();
    private readonly Mock<IDagEngine> _dagEngine = new();
    private readonly Mock<IConditionEvaluator> _conditionEvaluator = new();
    private readonly Mock<IDomainEventDispatcher> _eventDispatcher = new();
    private readonly Mock<ILogger<TriggerBuildHandler>> _logger = new();

    private readonly TriggerBuildHandler _sut;

    public TriggerBuildHandlerTests()
    {
        _sut = new TriggerBuildHandler(
            _pipelines.Object,
            _builds.Object,
            _jobs.Object,
            _yamlParser.Object,
            _triggerMatcher.Object,
            _dagEngine.Object,
            _conditionEvaluator.Object,
            _eventDispatcher.Object,
            _logger.Object
        );
    }

    [Fact]
    public async Task HandleAsync_WithNoYaml_CreatesBuildWithoutStages()
    {
        var pipeline = Pipeline.Create("test", "repo", "main");
        _pipelines.Setup(p => p.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pipeline);

        var command = new TriggerBuildCommand(Guid.NewGuid(), "push", "abc123", "user", "main");

        var result = await _sut.HandleAsync(command);

        Assert.NotEqual(Guid.Empty, result);
        _builds.Verify(b => b.AddAsync(It.IsAny<Build>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_TriggerDoesNotMatch_ReturnsEmptyGuid()
    {
        var pipeline = Pipeline.Create("test", "repo", "main");
        pipeline.UpdateYaml("name: test\nstages: []");
        _pipelines.Setup(p => p.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pipeline);

        var def = new PipelineDefinition("test", null, new(), new());
        _yamlParser.Setup(p => p.Parse(It.IsAny<string>())).Returns(def);
        _triggerMatcher.Setup(m => m.Matches(It.IsAny<TriggerConfig?>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        var command = new TriggerBuildCommand(Guid.NewGuid(), "push", "abc123", "user", "main");

        var result = await _sut.HandleAsync(command);

        Assert.Equal(Guid.Empty, result);
    }

    [Fact]
    public async Task HandleAsync_PipelineNotFound_Throws()
    {
        _pipelines.Setup(p => p.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pipeline?)null);

        var command = new TriggerBuildCommand(Guid.NewGuid(), "push", "abc123", "user", "main");

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.HandleAsync(command));
    }
}
