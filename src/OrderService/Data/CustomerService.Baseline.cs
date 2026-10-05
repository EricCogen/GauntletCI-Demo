using OrderService.Abstractions;
using OrderService.Persistence;

namespace OrderService.Data;

public class CustomerService
{
    private readonly ICustomerRepository _repository;
    private readonly IDistributedCache _cache;

    public CustomerService(ICustomerRepository repository, IDistributedCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<Customer?> GetCustomerAsync(Guid customerId, CancellationToken ct)
    {
        var cacheKey = $"customer:{customerId}";
        var cached = await _cache.GetAsync<Customer>(cacheKey, ct);
        if (cached != null)
            return cached;

        var customer = await _repository.GetAsync(customerId, ct);
        if (customer != null)
        {
            await _cache.SetAsync(cacheKey, customer, TimeSpan.FromHours(1), ct);
        }
        return customer;
    }
}

public record Customer(Guid Id, string Name, string Email);

public interface ICustomerRepository
{
    Task<Customer?> GetAsync(Guid customerId, CancellationToken ct);
}

public interface IDistributedCache
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct);
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct);
}
