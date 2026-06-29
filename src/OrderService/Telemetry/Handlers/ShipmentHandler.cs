using System.Threading.Tasks;

namespace OrderService.Telemetry.Handlers;

public class ShipmentHandler
{
    // BASELINE: No shared mutable state
    private readonly IShippingService _service;

    public ShipmentHandler(IShippingService service)
    {
        _service = service;
    }

    public async Task HandleAsync(ShipmentRequest request)
    {
        await _service.DispatchAsync(request);
    }
}

public interface IShippingService 
{ 
    Task DispatchAsync(ShipmentRequest req); 
}

public record ShipmentRequest(string TrackingId);
