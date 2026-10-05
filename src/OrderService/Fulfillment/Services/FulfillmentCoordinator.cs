using System;
using System.Threading.Tasks;

namespace OrderService.Fulfillment.Services;

public class FulfillmentCoordinator
{
    private readonly IAuditLogger _auditLogger;
    private readonly IFulfillmentEngine _fulfillmentEngine;

    public FulfillmentCoordinator(IAuditLogger auditLogger, IFulfillmentEngine fulfillmentEngine)
    {
        _auditLogger = auditLogger;
        _fulfillmentEngine = fulfillmentEngine;
    }

    // BASELINE: Audit logging happens BEFORE execution
    public async Task<bool> CompleteOrderAsync(OrderContext context)
    {
        // High-integrity audit rule: Log intent BEFORE execution completes
        await _auditLogger.LogActionAsync("OrderFulfillment", context.UserId);

        var outcome = await _fulfillmentEngine.ProcessAsync(context);
        return outcome;
    }
}

public interface IAuditLogger 
{ 
    Task LogActionAsync(string action, string userId); 
}

public interface IFulfillmentEngine 
{ 
    Task<bool> ProcessAsync(OrderContext ctx); 
}

public record OrderContext(string OrderId, string UserId);
