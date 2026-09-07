using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace OrderService.Billing.Controllers;

[ApiController]
[Route("api/billing")]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;

    public BillingController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    // BASELINE: Authorization attribute is present
    [Authorize(Roles = "BillingAdmin")]
    [HttpPost("refunds/{id}")]
    public async Task<IActionResult> ProcessRefund(Guid id, [FromBody] RefundRequest request)
    {
        var result = await _billingService.ExecuteRefundAsync(id, request.Amount);
        return Ok(result);
    }
}

public interface IBillingService
{
    Task<bool> ExecuteRefundAsync(Guid id, decimal amount);
}

public record RefundRequest(decimal Amount);
