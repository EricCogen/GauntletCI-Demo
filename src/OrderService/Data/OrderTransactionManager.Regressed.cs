using OrderService.Abstractions;
using OrderService.Persistence;

namespace OrderService.Data;

public class OrderTransactionManager
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<OrderTransactionManager> _logger;

    public OrderTransactionManager(IOrderRepository repository, ILogger<OrderTransactionManager> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> ProcessOrderAsync(Order order, IDbTransaction tx, CancellationToken ct)
    {
        try
        {
            await tx.BeginAsync(ct);
            await _repository.UpdateInventoryAsync(order, ct);
            await _repository.CreateShipmentAsync(order, ct);
            await tx.CommitAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Order failed: {ex.Message}");
            return false;
        }
        finally
        {
            await tx.RollbackAsync(ct);
        }
    }
}

public interface IDbTransaction
{
    Task BeginAsync(CancellationToken ct);
    Task CommitAsync(CancellationToken ct);
    Task RollbackAsync(CancellationToken ct);
}
