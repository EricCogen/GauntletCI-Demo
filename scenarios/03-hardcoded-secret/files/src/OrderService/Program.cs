using Microsoft.Extensions.Logging;
using OrderService;
using OrderService.Models;

using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
using var http = new HttpClient();

var apiBaseUrl = "https://payments.example.test";
var apiKey = "DEMO_FAKE_KEY_aB3xYz9KqRtVwN2mPpL5jH7bC4dE6fG8";

var payments = new PaymentClient(http, loggerFactory.CreateLogger<PaymentClient>(), apiBaseUrl, apiKey);
var processor = new OrderProcessor(payments, loggerFactory.CreateLogger<OrderProcessor>());

var order = new Order(Id: "ord-1", CustomerId: "cust-7", Amount: 49.99m, Currency: "USD");
var result = await processor.ProcessAsync(order);

Console.WriteLine(result.Success ? "OK" : $"FAIL: {result.Reason}");
return result.Success ? 0 : 1;
