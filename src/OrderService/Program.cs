using OrderService.Abstractions;
using OrderService.Background;
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
builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("Stripe"));
builder.Services.AddSingleton<IPaymentClient, PaymentClient>();
builder.Services.AddScoped<OrderProcessor>();

builder.Services.AddSingleton<OrderReminderBackgroundService>();
builder.Services.AddHostedService(sp =>
    sp.GetRequiredService<OrderReminderBackgroundService>());
builder.Services.AddScoped<IOrderEventEmitter, OrderEventEmitter>();

var app = builder.Build();
app.MapControllers();
app.MapGet("/", () => "OrderService demo");

app.Run();

public partial class Program { }
