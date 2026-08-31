using System.Threading.Tasks;

namespace OrderService.Telemetry.Handlers;

public class ShipmentHandler
{
    // REGRESSION: Unsynchronized static field mutated in async method
    private static int _globalShipmentCount = 0;
    private readonly IShippingService _service;

    public ShipmentHandler(IShippingService service)
    {
        _service = service;
    }

    public async Task HandleAsync(ShipmentRequest request)
    {
        // Concurrency regression: Bare unsynchronized modification inside an async Task context
        _globalShipmentCount++;

        await _service.DispatchAsync(request);
    }
}
