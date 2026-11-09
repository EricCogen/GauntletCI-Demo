using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Contracts.Interfaces;

// BASELINE: Public interface with CancellationToken parameter
public interface INotificationDispatcher
{
    Task BroadcastAsync(string headline, string payload, CancellationToken ct = default);
}
