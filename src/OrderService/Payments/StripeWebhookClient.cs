namespace OrderService.Payments;

public sealed class StripeWebhookClient
{
    private readonly HttpClient _http = new HttpClient();

    public Task<HttpResponseMessage> AcknowledgeAsync(Uri callback, CancellationToken ct)
        => _http.PostAsync(callback, content: null, ct);
}
