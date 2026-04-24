using OrderService.Domain;

namespace OrderService.Payments;

public sealed record PaymentRequest(Guid OrderId, Money Amount, string CustomerEmail);
