using Orchestrator.Domain.Interfaces;
using Orchestrator.Domain.Services;

namespace Orchestrator.Tests.Domain;

public sealed class PipelineTriggerMatcherTests
{
    private readonly PipelineTriggerMatcher _sut = new();

    [Fact]
    public void Matches_NullTrigger_ReturnsTrue()
    {
        Assert.True(_sut.Matches(null, "main", "push"));
        Assert.True(_sut.Matches(null, "feature", "pull_request"));
    }

    [Fact]
    public void Matches_ExactBranch_ReturnsCorrectResult()
    {
        var trigger = new TriggerConfig(["main"], ["push"]);
        Assert.True(_sut.Matches(trigger, "main", "push"));
        Assert.False(_sut.Matches(trigger, "develop", "push"));
    }

    [Fact]
    public void Matches_GlobReleasePrefix_MatchesCorrectly()
    {
        var trigger = new TriggerConfig(["release/*"], ["push"]);
        Assert.True(_sut.Matches(trigger, "release/v1", "push"));
        Assert.True(_sut.Matches(trigger, "release/2.0", "push"));
        Assert.False(_sut.Matches(trigger, "main", "push"));
        Assert.False(_sut.Matches(trigger, "release/v1/hotfix", "push"));
    }

    [Fact]
    public void Matches_GlobFeatureDoubleStar_MatchesCorrectly()
    {
        var trigger = new TriggerConfig(["feature/**"], ["push"]);
        Assert.True(_sut.Matches(trigger, "feature/new-ui", "push"));
        Assert.True(_sut.Matches(trigger, "feature/new-ui/subtask", "push"));
        Assert.False(_sut.Matches(trigger, "main", "push"));
    }

    [Fact]
    public void Matches_Wildcard_MatchesAnyBranch()
    {
        var trigger = new TriggerConfig(["*"], ["push"]);
        Assert.True(_sut.Matches(trigger, "main", "push"));
        Assert.True(_sut.Matches(trigger, "any-branch", "push"));
    }

    [Fact]
    public void Matches_EventFiltering_ReturnsCorrectResult()
    {
        var trigger = new TriggerConfig(["main"], ["push", "tag"]);
        Assert.True(_sut.Matches(trigger, "main", "push"));
        Assert.True(_sut.Matches(trigger, "main", "tag"));
        Assert.False(_sut.Matches(trigger, "main", "pull_request"));
    }

    [Fact]
    public void Matches_MultipleBranchesAndEvents_AllCombinationsWork()
    {
        var trigger = new TriggerConfig(["main", "develop", "release/*"], ["push", "pull_request"]);

        Assert.True(_sut.Matches(trigger, "main", "push"));
        Assert.True(_sut.Matches(trigger, "develop", "pull_request"));
        Assert.True(_sut.Matches(trigger, "release/1.0", "push"));
        Assert.False(_sut.Matches(trigger, "feature/x", "push"));
        Assert.False(_sut.Matches(trigger, "main", "tag"));
    }

    [Fact]
    public void Matches_EmptyBranchesOrEvents_DoesNotFilter()
    {
        var triggerNoBranches = new TriggerConfig([], ["push"]);
        Assert.True(_sut.Matches(triggerNoBranches, "any", "push"));

        var triggerNoEvents = new TriggerConfig(["main"], []);
        Assert.True(_sut.Matches(triggerNoEvents, "main", "any-event"));
    }
}
