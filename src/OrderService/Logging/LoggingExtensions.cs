using Microsoft.Extensions.Logging;

namespace OrderService.Logging;

public static class LoggingExtensions
{
    private static readonly Action<ILogger, Guid, string, Exception?> _orderProcessed =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Information,
            new EventId(1001, "OrderProcessed"),
            "Order {OrderId} processed with status {Status}");

    public static void OrderProcessed(this ILogger logger, Guid orderId, string status)
        => _orderProcessed(logger, orderId, status, null);
}
