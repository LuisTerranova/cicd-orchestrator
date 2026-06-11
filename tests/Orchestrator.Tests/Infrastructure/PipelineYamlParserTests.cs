using Orchestrator.Domain.Exceptions;
using Orchestrator.Infrastructure.Services;

namespace Orchestrator.Tests.Infrastructure;

public sealed class PipelineYamlParserTests
{
    private readonly YamlDotNetParser _sut = new();

    [Fact]
    public void Parse_ValidYaml_ReturnsPipelineDefinition()
    {
        var yaml = """
            name: test-pipeline
            stages:
              - name: build
                steps:
                  - name: build-app
                    run: dotnet build
            """;
        var result = _sut.Parse(yaml);
        Assert.Equal("test-pipeline", result.Name);
        Assert.Single(result.Stages);
    }

    [Fact]
    public void Parse_EmptyYaml_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => _sut.Parse(""));
    }

    [Fact]
    public void Parse_MissingName_ThrowsDomainException()
    {
        var yaml = """
            stages:
              - name: build
                steps:
                  - name: b
                    run: echo
            """;
        Assert.Throws<DomainException>(() => _sut.Parse(yaml));
    }

    [Fact]
    public void Parse_MissingStages_ThrowsDomainException()
    {
        var yaml = "name: test\nstages: []";
        Assert.Throws<DomainException>(() => _sut.Parse(yaml));
    }

    [Fact]
    public void Parse_DefaultValues_AreApplied()
    {
        var yaml = """
            name: test
            stages:
              - name: build
                steps:
                  - name: build-app
                    run: dotnet build
            """;
        var result = _sut.Parse(yaml);
        Assert.Null(result.Stages[0].Image);
        Assert.Null(result.Stages[0].Condition);
        Assert.Empty(result.Stages[0].DependsOn);
    }

    [Fact]
    public void Parse_TimeoutParsing_SupportsUnits()
    {
        var yaml = """
            name: test
            stages:
              - name: build
                timeout: 5m
                steps:
                  - name: b
                    run: echo
            """;
        var result = _sut.Parse(yaml);
        Assert.Equal(TimeSpan.FromMinutes(5), result.Stages[0].Timeout);
    }

    [Fact]
    public void Parse_CustomShell_IsPreserved()
    {
        var yaml = """
            name: test
            stages:
              - name: build
                steps:
                  - name: b
                    run: echo hi
                    shell: zsh
            """;
        var result = _sut.Parse(yaml);
        Assert.Equal("zsh", result.Stages[0].Steps[0].Shell);
    }

    [Fact]
    public void Parse_ContinueOnError_DefaultsToFalse()
    {
        var yaml = """
            name: test
            stages:
              - name: build
                steps:
                  - name: b
                    run: echo hi
            """;
        var result = _sut.Parse(yaml);
        Assert.False(result.Stages[0].Steps[0].ContinueOnError);
    }

    [Fact]
    public void Parse_EnvironmentVariables_ArePreserved()
    {
        var yaml = """
            name: test
            env:
              NODE_ENV: production
              LOG_LEVEL: debug
            stages:
              - name: build
                steps:
                  - name: b
                    run: echo
            """;
        var result = _sut.Parse(yaml);
        Assert.Equal("production", result.Env["NODE_ENV"]);
        Assert.Equal("debug", result.Env["LOG_LEVEL"]);
    }

    [Fact]
    public void Parse_Trigger_WithBranchesAndEvents()
    {
        var yaml = """
            name: test
            trigger:
              branches:
                - main
                - release/*
              events:
                - push
                - tag
            stages:
              - name: build
                steps:
                  - name: b
                    run: echo
            """;
        var result = _sut.Parse(yaml);
        Assert.NotNull(result.Trigger);
        Assert.Contains("main", result.Trigger.Branches);
        Assert.Contains("release/*", result.Trigger.Branches);
        Assert.Contains("push", result.Trigger.Events);
    }

    [Fact]
    public void Parse_StageCondition_IsPreserved()
    {
        var yaml = """
            name: test
            stages:
              - name: deploy
                condition: branch == "main"
                steps:
                  - name: deploy-app
                    run: echo deploy
            """;
        var result = _sut.Parse(yaml);
        Assert.Equal("branch == \"main\"", result.Stages[0].Condition);
    }
}
