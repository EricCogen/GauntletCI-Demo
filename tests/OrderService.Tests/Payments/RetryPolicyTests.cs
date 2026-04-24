using OrderService.Payments;
using Xunit;

namespace OrderService.Tests.Payments;

public class RetryPolicyTests
{
    [Fact]
    public async Task Returns_value_on_first_success()
    {
        var policy = new RetryPolicy(3, TimeSpan.Zero);
        var result = await policy.ExecuteAsync(_ => Task.FromResult(42));
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task Retries_until_success()
    {
        var policy = new RetryPolicy(3, TimeSpan.Zero);
        var attempts = 0;
        var result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            if (attempts < 3)
            {
                throw new InvalidOperationException("flaky");
            }
            return Task.FromResult("ok");
        });
        Assert.Equal("ok", result);
        Assert.Equal(3, attempts);
    }

    [Fact]
    public async Task Throws_after_max_attempts()
    {
        var policy = new RetryPolicy(2, TimeSpan.Zero);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            policy.ExecuteAsync<int>(_ => throw new InvalidOperationException("boom")));
    }
}
