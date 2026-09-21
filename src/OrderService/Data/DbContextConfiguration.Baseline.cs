using Microsoft.EntityFrameworkCore;
using OrderService.Abstractions;

namespace OrderService.Data;

public class DbContextConfiguration
{
    public static DbContextOptions<OrderContext> GetOptions()
    {
        var connectionString = "Data Source=localhost;Initial Catalog=OrderDb;" +
            "Integrated Security=true;Pooling=true;Max Pool Size=100;";

        var options = new DbContextOptionsBuilder<OrderContext>()
            .UseSqlServer(connectionString)
            .Build();

        return options;
    }
}

public class OrderContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    public OrderContext(DbContextOptions<OrderContext> options) : base(options)
    {
    }
}

public class Order
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; }
}
