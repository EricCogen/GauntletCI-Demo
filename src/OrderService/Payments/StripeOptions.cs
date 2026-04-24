namespace OrderService.Payments;

public sealed class StripeOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.stripe.com";
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
}
