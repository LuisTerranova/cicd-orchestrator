using Microsoft.Extensions.Logging;

namespace Orchestrator.Runner.Configuration;

public sealed class RunnerOptionsValidator
{
    private readonly ILogger<RunnerOptionsValidator> _logger;

    public RunnerOptionsValidator(ILogger<RunnerOptionsValidator> logger)
    {
        _logger = logger;
    }

    public List<string> Validate(RunnerOptions options)
    {
        var errors = new List<string>();

        // ServerUrl must be a valid absolute URL
        if (
            !Uri.TryCreate(options.ServerUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != "http" && uri.Scheme != "https")
        )
        {
            errors.Add($"ServerUrl must be a valid HTTP/HTTPS URL. Got: '{options.ServerUrl}'");
        }

        // Concurrency must be at least 1
        if (options.Concurrency < 1)
        {
            errors.Add($"Concurrency must be >= 1. Got: {options.Concurrency}");
        }

        // EncryptionKey empty is a warning, not a fatal error
        if (string.IsNullOrEmpty(options.EncryptionKey))
        {
            _logger.LogWarning("EncryptionKey is empty — secrets will not be decryptable");
        }

        return errors;
    }
}
