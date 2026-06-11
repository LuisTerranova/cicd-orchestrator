using Orchestrator.Domain.Exceptions;
using Orchestrator.Domain.Interfaces;
using Orchestrator.Domain.Services;

namespace Orchestrator.Tests.Domain;

public sealed class DagEngineTests
{
    private readonly DagEngine _sut = new();

    [Fact]
    public void BuildLayers_SingleStage_ReturnsOneLayer()
    {
        var stages = new List<StageDefinition>
        {
            new("build", [], null, null, TimeSpan.FromMinutes(30), []),
        };
        var result = _sut.BuildLayers(stages);
        Assert.Single(result.Layers);
        Assert.Single(result.Layers[0]);
    }

    [Fact]
    public void BuildLayers_NoDependencies_AllInLayerZero()
    {
        var stages = new List<StageDefinition>
        {
            new("build", [], null, null, TimeSpan.FromMinutes(30), []),
            new("test", [], null, null, TimeSpan.FromMinutes(30), []),
        };
        var result = _sut.BuildLayers(stages);
        Assert.Single(result.Layers);
        Assert.Equal(2, result.Layers[0].Count);
    }

    [Fact]
    public void BuildLayers_LinearDependency_CreatesCorrectLayers()
    {
        var stages = new List<StageDefinition>
        {
            new("build", [], null, null, TimeSpan.FromMinutes(30), []),
            new("test", ["build"], null, null, TimeSpan.FromMinutes(30), []),
            new("deploy", ["test"], null, null, TimeSpan.FromMinutes(30), []),
        };
        var result = _sut.BuildLayers(stages);
        Assert.Equal(3, result.Layers.Count);
        Assert.Equal("build", result.Layers[0][0].Name);
        Assert.Equal("test", result.Layers[1][0].Name);
        Assert.Equal("deploy", result.Layers[2][0].Name);
    }

    [Fact]
    public void BuildLayers_ParallelDependencies_CreatesCorrectLayers()
    {
        var stages = new List<StageDefinition>
        {
            new("lint", [], null, null, TimeSpan.FromMinutes(30), []),
            new("build", [], null, null, TimeSpan.FromMinutes(30), []),
            new("test", ["lint", "build"], null, null, TimeSpan.FromMinutes(30), []),
        };
        var result = _sut.BuildLayers(stages);
        Assert.Equal(2, result.Layers.Count);
        Assert.Equal(2, result.Layers[0].Count);
        Assert.Equal("test", result.Layers[1][0].Name);
    }

    [Fact]
    public void BuildLayers_CircularDependency_ThrowsDomainException()
    {
        var stages = new List<StageDefinition>
        {
            new("build", ["test"], null, null, TimeSpan.FromMinutes(30), []),
            new("test", ["deploy"], null, null, TimeSpan.FromMinutes(30), []),
            new("deploy", ["build"], null, null, TimeSpan.FromMinutes(30), []),
        };
        Assert.Throws<DomainException>(() => _sut.BuildLayers(stages));
    }

    [Fact]
    public void BuildLayers_UnknownDependency_ThrowsDomainException()
    {
        var stages = new List<StageDefinition>
        {
            new("build", ["nonexistent"], null, null, TimeSpan.FromMinutes(30), []),
        };
        var ex = Assert.Throws<DomainException>(() => _sut.BuildLayers(stages));
        Assert.Contains("unknown", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildLayers_EmptyStages_Succeeds()
    {
        var stages = new List<StageDefinition>();
        var result = _sut.BuildLayers(stages);
        Assert.Empty(result.Layers);
        Assert.Empty(result.TopologicalOrder);
    }

    [Fact]
    public void TopologicalOrder_RespectsDependencies()
    {
        var stages = new List<StageDefinition>
        {
            new("deploy", ["test"], null, null, TimeSpan.FromMinutes(30), []),
            new("test", ["build"], null, null, TimeSpan.FromMinutes(30), []),
            new("build", [], null, null, TimeSpan.FromMinutes(30), []),
        };
        var result = _sut.BuildLayers(stages);
        var order = result.TopologicalOrder.Select(s => s.Name).ToList();
        Assert.Equal("build", order[0]);
        Assert.Equal("test", order[1]);
        Assert.Equal("deploy", order[2]);
    }

    [Fact]
    public void BuildLayers_DiamondDependency_CorrectLayers()
    {
        var stages = new List<StageDefinition>
        {
            new("build", [], null, null, TimeSpan.FromMinutes(30), []),
            new("unit", ["build"], null, null, TimeSpan.FromMinutes(30), []),
            new("integration", ["build"], null, null, TimeSpan.FromMinutes(30), []),
            new("deploy", ["unit", "integration"], null, null, TimeSpan.FromMinutes(30), []),
        };
        var result = _sut.BuildLayers(stages);
        Assert.Equal(3, result.Layers.Count);
        Assert.Single(result.Layers[0]); // build
        Assert.Equal(2, result.Layers[1].Count); // unit + integration
        Assert.Single(result.Layers[2]); // deploy
    }
}
