namespace OrderService.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
