using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace OrderService.Billing.Controllers;

[ApiController]
[Route("api/billing")]
public class BillingController : ControllerBase
{
    private readonly IMediator _mediator;

    public BillingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // REGRESSION: [Authorize] attribute was stripped during refactoring
    [HttpPost("refunds/{id}")]
    public async Task<IActionResult> ProcessRefund(Guid id, [FromBody] RefundRequest request)
    {
        var result = await _mediator.Send(new ProcessRefundCommand(id, request.Amount));
        return Ok(result);
    }
}

public record ProcessRefundCommand(Guid Id, decimal Amount) : IRequest<bool>;
public record RefundRequest(decimal Amount);
