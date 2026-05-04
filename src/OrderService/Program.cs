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
builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("Stripe"));
builder.Services.AddSingleton<IPaymentClient, PaymentClient>();
builder.Services.AddScoped<OrderProcessor>();

// Order-database connection — temporarily inlined while the
// secret-store wiring is being moved out of the legacy host.
const string ordersDbConnection =
    "Server=tcp:orders-db.internal;Database=Orders;Integrated Security=true;TrustServerCertificate=true";
builder.Services.AddSingleton(new OrdersDbConnectionString(ordersDbConnection));

var app = builder.Build();
app.MapControllers();
app.MapGet("/", () => "OrderService demo");

app.Run();

public partial class Program { }

public sealed record OrdersDbConnectionString(string Value);
