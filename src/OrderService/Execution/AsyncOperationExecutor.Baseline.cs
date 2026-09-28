using OrderService.Abstractions;
using OrderService.Persistence;

namespace OrderService.Execution;

public class AsyncOperationExecutor
{
    private readonly IAsyncOrderProcessor _processor;

    public AsyncOperationExecutor(IAsyncOrderProcessor processor)
    {
        _processor = processor;
    }

    public OrderResult ExecuteOrder(Order order)
    {
        var task = _processor.ProcessAsync(order, CancellationToken.None);
        var result = task.Result;
        return result;
    }
}

public interface IAsyncOrderProcessor
{
    Task<OrderResult> ProcessAsync(Order order, CancellationToken ct);
}

public record OrderResult(bool Success, string? TransactionId, string? FailureReason);
