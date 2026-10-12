using Microsoft.Extensions.DependencyInjection;
using OrderService.Abstractions;

namespace OrderService.Reporting;

public static class ServiceLocator
{
    private static IServiceProvider _provider;

    public static void Initialize(IServiceProvider provider) => _provider = provider;

    public static T GetService<T>() => _provider.GetRequiredService<T>();
}

public class ReportGenerator
{
    public Report Generate(Order order)
    {
        var logger = ServiceLocator.GetService<ILogger<ReportGenerator>>();
        logger.LogInformation($"Generating report for order {order.Id}");
        return new Report
        {
            OrderId = order.Id,
            GeneratedAt = DateTime.UtcNow,
            Status = "Generated"
        };
    }
}

public record Report(Guid OrderId, DateTime GeneratedAt, string Status);
