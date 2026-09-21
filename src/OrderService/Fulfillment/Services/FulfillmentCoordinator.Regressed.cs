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

    // REGRESSION: Audit logging moved AFTER execution. If ProcessAsync throws, log is bypassed.
    public async Task<bool> CompleteOrderAsync(OrderContext context)
    {
        // Behavioral drift: Execution flipped. If ProcessAsync throws, log is bypassed entirely.
        var outcome = await _fulfillmentEngine.ProcessAsync(context);

        await _auditLogger.LogActionAsync("OrderFulfillment", context.UserId);

        return outcome;
    }
}
