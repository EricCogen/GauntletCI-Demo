using OrderService.Abstractions;
using OrderService.Persistence;

namespace OrderService.Data;

public class CustomerService
{
    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<Customer?> GetCustomerAsync(Guid customerId, CancellationToken ct)
    {
        return await _repository.GetAsync(customerId, ct);
    }
}

public record Customer(Guid Id, string Name, string Email);

public interface ICustomerRepository
{
    Task<Customer?> GetAsync(Guid customerId, CancellationToken ct);
}
