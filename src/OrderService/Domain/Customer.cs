namespace OrderService.Domain;

public sealed record Customer(Guid Id, string Email, string DisplayName);
