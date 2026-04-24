using Microsoft.Extensions.Logging;
using OrderService;
using OrderService.Models;

using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
using var http = new HttpClient();

var apiBaseUrl = Environment.GetEnvironmentVariable("PAYMENT_API_URL")
    ?? "https://payments.example.test";

var payments = new PaymentClient(http, loggerFactory.CreateLogger<PaymentClient>(), apiBaseUrl);
var processor = new OrderProcessor(payments, loggerFactory.CreateLogger<OrderProcessor>());

var order = new Order(Id: "ord-1", CustomerId: "cust-7", Amount: 49.99m, Currency: "USD");
var result = await processor.ProcessAsync(order);

Console.WriteLine(result.Success ? "OK" : $"FAIL: {result.Reason}");
return result.Success ? 0 : 1;
