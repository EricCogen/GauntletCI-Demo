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

    public async Task<OrderResult> ExecuteOrderAsync(Order order)
    {
        var result = await _processor.ProcessAsync(order, CancellationToken.None);
        return result;
    }
}

public interface IAsyncOrderProcessor
{
    Task<OrderResult> ProcessAsync(Order order, CancellationToken ct);
}

public record OrderResult(bool Success, string? TransactionId, string? FailureReason);
