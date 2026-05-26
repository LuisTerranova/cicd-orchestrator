using System.CommandLine;

namespace Orchestrator.Runner.Cli;

public sealed class CliRootCommand
{
    private readonly Option<string> _configOpt = new("--config");
    private readonly Option<string> _serverUrlOpt = new("--server-url");
    private readonly Option<int> _concurrencyOpt = new("--concurrency");
    private readonly Option<string[]> _labelsOpt = new("--labels");
    private readonly Option<string> _logLevelOpt = new("--log-level");
    private readonly Option<bool> _versionOpt = new("--version");
    private readonly Option<bool> _dryRunOpt = new("--dry-run");
    private readonly Option<string> _registrationTokenOpt = new("--registration-token");

    private readonly Dictionary<string, Option> _options;

    public CliRootCommand()
    {
        _options = new Dictionary<string, Option>
        {
            ["--config"] = _configOpt,
            ["--server-url"] = _serverUrlOpt,
            ["--concurrency"] = _concurrencyOpt,
            ["--labels"] = _labelsOpt,
            ["--log-level"] = _logLevelOpt,
            ["--version"] = _versionOpt,
            ["--dry-run"] = _dryRunOpt,
            ["--registration-token"] = _registrationTokenOpt,
        };
    }

    public CliParseResult Invoke(string[] args)
    {
        var root = new RootCommand("CI/CD Orchestrator Runner");
        foreach (var opt in _options.Values)
            root.Add(opt);

        var result = root.Parse(args);
        return new CliParseResult(result, _options);
    }
}

public sealed record CliParseResult
{
    private readonly ParseResult _result;
    private readonly Dictionary<string, Option> _options;

    public CliParseResult(ParseResult result, Dictionary<string, Option> options)
    {
        _result = result;
        _options = options;
    }

    public T? GetValue<T>(string name)
    {
        return _result.GetValue<T>(name);
    }

    public bool HasOption(string name)
    {
        return _options.TryGetValue(name, out var opt) && _result.GetResult(opt) is not null;
    }
}
