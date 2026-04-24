using OrderService.Abstractions;

namespace OrderService.Tests.Fakes;

public sealed class FixedClock : IClock
{
    public DateTimeOffset UtcNow { get; set; } = new(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
}
