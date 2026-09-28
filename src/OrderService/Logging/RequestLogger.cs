using Microsoft.Extensions.Logging;

namespace OrderService.Logging;

public sealed class RequestLogger
{
    private readonly ILogger<RequestLogger> _logger;

    public RequestLogger(ILogger<RequestLogger> logger)
    {
        _logger = logger;
    }

    public void LogIncoming(string method, string path, string? correlationId)
        => _logger.LogInformation(
            "Http {Method} {Path} correlationId={CorrelationId}",
            method, path, correlationId ?? "-");
}
