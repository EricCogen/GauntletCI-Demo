using Microsoft.Extensions.DependencyInjection;
using OrderService.Abstractions;
using OrderService.Data;

namespace OrderService.Logging;

public class LoggingServiceConfiguration
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<UserContext>();
        services.AddSingleton<LoggingService>();
    }
}

public class LoggingService
{
    private readonly UserContext _userContext;

    public LoggingService(UserContext userContext)
    {
        _userContext = userContext;
    }

    public void LogUserAction(string action)
    {
        System.Console.WriteLine($"User {_userContext.UserId}: {action}");
    }
}

public class UserContext
{
    public Guid UserId { get; set; }
    public string TenantId { get; set; }
}
