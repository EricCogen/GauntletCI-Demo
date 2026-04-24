namespace OrderService.Payments;

public sealed class RetryPolicy
{
    private readonly int _maxAttempts;
    private readonly TimeSpan _delay;

    public RetryPolicy(int maxAttempts = 3, TimeSpan? delay = null)
    {
        if (maxAttempts < 1) throw new ArgumentOutOfRangeException(nameof(maxAttempts));
        _maxAttempts = maxAttempts;
        _delay = delay ?? TimeSpan.FromMilliseconds(50);
    }

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct = default)
    {
        Exception? last = null;
        for (var attempt = 1; attempt <= _maxAttempts; attempt++)
        {
            try
            {
                return await action(ct).ConfigureAwait(false);
            }
            catch (Exception ex) when (attempt < _maxAttempts)
            {
                last = ex;
                await Task.Delay(_delay, ct).ConfigureAwait(false);
            }
        }
        throw last ?? new InvalidOperationException("RetryPolicy exhausted with no captured exception.");
    }
}
