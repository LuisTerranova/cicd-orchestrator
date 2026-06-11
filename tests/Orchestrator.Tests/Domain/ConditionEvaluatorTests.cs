using Orchestrator.Domain.Interfaces;
using Orchestrator.Infrastructure.Services;

namespace Orchestrator.Tests.Domain;

public sealed class ConditionEvaluatorTests
{
    private readonly ConditionEvaluator _sut = new();
    private readonly BuildContext _mainPush = new("main", "push", "user", "repo", null);
    private readonly BuildContext _devPr = new("develop", "pull_request", "user", "repo", null);
    private readonly BuildContext _releaseTag = new("release/v1", "tag", "user", "repo", "v1.0");

    [Fact]
    public void Evaluate_EmptyExpression_ReturnsTrue()
    {
        Assert.True(_sut.Evaluate("", _mainPush));
        Assert.True(_sut.Evaluate(null!, _mainPush));
    }

    [Fact]
    public void Evaluate_EqualsOperator_ReturnsCorrectResult()
    {
        Assert.True(_sut.Evaluate("branch == \"main\"", _mainPush));
        Assert.False(_sut.Evaluate("branch == \"develop\"", _mainPush));
    }

    [Fact]
    public void Evaluate_NotEqualsOperator_ReturnsCorrectResult()
    {
        Assert.True(_sut.Evaluate("event != \"tag\"", _mainPush));
        Assert.False(_sut.Evaluate("event != \"push\"", _mainPush));
    }

    [Fact]
    public void Evaluate_AndOperator_BothConditionsMustBeTrue()
    {
        Assert.True(_sut.Evaluate("branch == \"main\" && event == \"push\"", _mainPush));
        Assert.False(_sut.Evaluate("branch == \"main\" && event == \"pull_request\"", _mainPush));
    }

    [Fact]
    public void Evaluate_OrOperator_EitherConditionCanBeTrue()
    {
        Assert.True(_sut.Evaluate("branch == \"main\" || branch == \"develop\"", _mainPush));
        Assert.False(_sut.Evaluate("branch == \"feature\" || branch == \"hotfix\"", _mainPush));
    }

    [Fact]
    public void Evaluate_InOperator_ReturnsCorrectResult()
    {
        Assert.True(_sut.Evaluate("branch in [\"main\", \"develop\"]", _mainPush));
        Assert.False(_sut.Evaluate("branch in [\"feature\", \"hotfix\"]", _mainPush));
    }

    [Fact]
    public void Evaluate_NotOperator_InvertsResult()
    {
        Assert.True(_sut.Evaluate("! (branch == \"develop\")", _mainPush));
        Assert.False(_sut.Evaluate("! (branch == \"main\")", _mainPush));
    }

    [Fact]
    public void Evaluate_ParenthesizedGroups_RespectsPrecedence()
    {
        Assert.True(_sut.Evaluate("(branch == \"main\") && (event == \"push\")", _mainPush));
        Assert.True(_sut.Evaluate("branch == \"main\" && (event == \"tag\" || event == \"push\")", _mainPush));
        Assert.False(_sut.Evaluate("(branch == \"develop\") && (event == \"push\")", _mainPush));
    }

    [Fact]
    public void Evaluate_ContainsFunction_ReturnsCorrectResult()
    {
        Assert.True(_sut.Evaluate("contains(branch, \"main\")", _mainPush));
        Assert.False(_sut.Evaluate("contains(branch, \"feature\")", _mainPush));
    }

    [Fact]
    public void Evaluate_StartsWithFunction_ReturnsCorrectResult()
    {
        Assert.True(_sut.Evaluate("startsWith(branch, \"release/\")", _releaseTag));
        Assert.False(_sut.Evaluate("startsWith(branch, \"release/\")", _mainPush));
    }

    [Fact]
    public void Evaluate_BuiltInVars_WorksCorrectly()
    {
        Assert.True(_sut.Evaluate("is_main", _mainPush));
        Assert.False(_sut.Evaluate("is_main", _devPr));
        Assert.True(_sut.Evaluate("is_pr", _devPr));
        Assert.False(_sut.Evaluate("is_pr", _mainPush));
        Assert.True(_sut.Evaluate("!is_pr", _mainPush));
    }

    [Fact]
    public void Evaluate_UndefinedVariable_ReturnsFalse()
    {
        Assert.False(_sut.Evaluate("nonexistent", _mainPush));
    }

    [Fact]
    public void Evaluate_BareVariable_ReturnsFalse()
    {
        Assert.False(_sut.Evaluate("branch", _mainPush));
    }

    [Fact]
    public void Evaluate_TagVariable_WorksCorrectly()
    {
        Assert.Equal("v1.0", _releaseTag.Tag);
        Assert.True(_sut.Evaluate("tag == \"v1.0\"", _releaseTag));
        Assert.False(_sut.Evaluate("tag == \"v2.0\"", _releaseTag));
    }
}
