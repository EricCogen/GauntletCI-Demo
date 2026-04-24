using Microsoft.AspNetCore.Http;

namespace OrderService.Abstractions;

public sealed class HttpCorrelationIdAccessor : ICorrelationIdAccessor
{
    private const string HeaderName = "X-Correlation-Id";
    private readonly IHttpContextAccessor _accessor;

    public HttpCorrelationIdAccessor(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public string? CorrelationId
        => _accessor.HttpContext?.Request.Headers[HeaderName].FirstOrDefault();
}
