namespace OrderService.Abstractions;

public interface ICorrelationIdAccessor
{
    string? CorrelationId { get; }
}
