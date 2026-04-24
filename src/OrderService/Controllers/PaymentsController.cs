using Microsoft.AspNetCore.Mvc;
using OrderService.Domain;
using OrderService.Payments;

namespace OrderService.Controllers;

[ApiController]
[Route("payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly IPaymentClient _payments;

    public PaymentsController(IPaymentClient payments)
    {
        _payments = payments;
    }

    [HttpPost("charge")]
    public async Task<ActionResult<PaymentResult>> Charge([FromBody] ChargeRequest request, CancellationToken ct)
    {
        var result = await _payments.ChargeAsync(
            new PaymentRequest(request.OrderId, new Money(request.Amount, request.Currency), request.CustomerEmail),
            ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }
}

public sealed record ChargeRequest(Guid OrderId, decimal Amount, string Currency, string CustomerEmail);
