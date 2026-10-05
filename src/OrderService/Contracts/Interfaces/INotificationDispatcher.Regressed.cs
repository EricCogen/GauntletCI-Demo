using System.Threading.Tasks;

namespace OrderService.Contracts.Interfaces;

// REGRESSION: Structural break - CancellationToken parameter removed completely
public interface INotificationDispatcher
{
    Task BroadcastAsync(string headline, string payload);
}
