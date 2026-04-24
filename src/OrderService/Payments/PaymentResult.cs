namespace OrderService.Payments;

public sealed record PaymentResult(bool Success, string? AuthorizationCode, string? FailureReason);
