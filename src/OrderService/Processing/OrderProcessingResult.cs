namespace OrderService.Processing;

public sealed record OrderProcessingResult(bool Success, string? AuthorizationCode, string? FailureReason);
