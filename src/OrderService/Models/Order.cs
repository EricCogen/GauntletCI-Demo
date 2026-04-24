namespace OrderService.Models;

public sealed record Order(
    string Id,
    string CustomerId,
    decimal Amount,
    string Currency);
