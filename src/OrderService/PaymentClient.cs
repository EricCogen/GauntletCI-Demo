using Microsoft.Extensions.Logging;
using OrderService.Models;

namespace OrderService;

public sealed class PaymentClient
{
    private readonly HttpClient _http;
    private readonly ILogger<PaymentClient> _logger;
    private readonly string _apiBaseUrl;
    private readonly string _apiKey;

    public PaymentClient(HttpClient http, ILogger<PaymentClient> logger, string apiBaseUrl, string apiKey)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _apiBaseUrl = apiBaseUrl ?? throw new ArgumentNullException(nameof(apiBaseUrl));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
    }

    public async Task<bool> ChargeAsync(Order order, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(order);

        using var req = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/charges");
        req.Headers.Add("X-Api-Key", _apiKey);
        req.Content = new StringContent(
            $"{{\"order\":\"{order.Id}\",\"amount\":{order.Amount},\"currency\":\"{order.Currency}\"}}",
            System.Text.Encoding.UTF8,
            "application/json");

        using var resp = await _http.SendAsync(req, ct).ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Payment failed for order {OrderId}: {StatusCode}",
                order.Id, (int)resp.StatusCode);
            return false;
        }

        return true;
    }
}
