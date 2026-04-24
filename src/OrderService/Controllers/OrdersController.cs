using Microsoft.AspNetCore.Mvc;
using OrderService.Abstractions;
using OrderService.Domain;
using OrderService.Persistence;
using OrderService.Processing;

namespace OrderService.Controllers;

[ApiController]
[Route("orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderRepository _repo;
    private readonly OrderProcessor _processor;
    private readonly IClock _clock;

    public OrdersController(IOrderRepository repo, OrderProcessor processor, IClock clock)
    {
        _repo = repo;
        _processor = processor;
        _clock = clock;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var customer = new Customer(request.CustomerId, request.CustomerEmail, request.CustomerName);
        var items = request.Items
            .Select(i => new OrderItem(i.Sku, i.Quantity, new Money(i.UnitAmount, request.Currency)))
            .ToList();
        var order = new Order(Guid.NewGuid(), customer, items, _clock.UtcNow);
        await _repo.AddAsync(order, ct);
        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Order>> Get(Guid id, CancellationToken ct)
    {
        var order = await _repo.GetAsync(id, ct);
        return order is null ? NotFound() : order;
    }

    [HttpPost("{id:guid}/process")]
    public async Task<ActionResult<OrderProcessingResult>> Process(Guid id, CancellationToken ct)
    {
        var result = await _processor.ProcessAsync(id, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }
}

public sealed record CreateOrderRequest(
    Guid CustomerId,
    string CustomerEmail,
    string CustomerName,
    string Currency,
    IReadOnlyList<CreateOrderItem> Items);

public sealed record CreateOrderItem(string Sku, int Quantity, decimal UnitAmount);
