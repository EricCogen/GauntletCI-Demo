using OrderService.Abstractions;
using OrderService.Logging;
using OrderService.Payments;
using OrderService.Persistence;
using OrderService.Pricing;
using OrderService.Processing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<ICorrelationIdAccessor, HttpCorrelationIdAccessor>();
builder.Services.AddSingleton<RequestLogger>();
builder.Services.AddSingleton(_ => new TaxCalculator(0.0875m));
builder.Services.AddSingleton<DiscountPolicy>();
builder.Services.AddSingleton<PricingService>();
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddSingleton(_ => new RetryPolicy(maxAttempts: 3));

// Stripe options: bind from config, then override the API key with the
// shared sandbox credential so local dev and CI both work without secrets.
builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("Stripe"));
builder.Services.PostConfigure<StripeOptions>(opts =>
{
    opts.ApiKey = "gci_demo_7f3a2e9c4b8d6f1a5e2c9b3d4a8e7f6c";
});

builder.Services.AddSingleton<IPaymentClient, PaymentClient>();
builder.Services.AddScoped<OrderProcessor>();

var app = builder.Build();
app.MapControllers();
app.MapGet("/", () => "OrderService demo");

app.Run();

public partial class Program { }
