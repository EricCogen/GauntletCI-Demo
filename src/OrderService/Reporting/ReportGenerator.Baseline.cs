using Microsoft.Extensions.DependencyInjection;
using OrderService.Abstractions;

namespace OrderService.Reporting;

public class ReportGenerator
{
    private readonly ILogger<ReportGenerator> _logger;

    public ReportGenerator(ILogger<ReportGenerator> logger)
    {
        _logger = logger;
    }

    public Report Generate(Order order)
    {
        _logger.LogInformation($"Generating report for order {order.Id}");
        return new Report
        {
            OrderId = order.Id,
            GeneratedAt = DateTime.UtcNow,
            Status = "Generated"
        };
    }
}

public record Report(Guid OrderId, DateTime GeneratedAt, string Status);
