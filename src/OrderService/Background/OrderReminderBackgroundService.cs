using Microsoft.Extensions.Hosting;

namespace OrderService.Background;

public interface IOrderEventEmitter
{
    void Emit(string eventName);
}

public sealed class OrderEventEmitter : IOrderEventEmitter
{
    public void Emit(string eventName) { }
}

public sealed class OrderReminderBackgroundService : BackgroundService
{
    private readonly IOrderEventEmitter _emitter;

    public OrderReminderBackgroundService(IOrderEventEmitter emitter)
    {
        _emitter = emitter;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _emitter.Emit("reminder.started");
        return Task.CompletedTask;
    }
}
