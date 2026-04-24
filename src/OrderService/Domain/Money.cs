namespace OrderService.Domain;

public readonly record struct Money(decimal Amount, string Currency)
{
    public static Money Zero(string currency) => new(0m, currency);

    public Money Add(Money other)
    {
        if (!string.Equals(Currency, other.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Currency mismatch: {Currency} vs {other.Currency}");
        }
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other) => Add(new Money(-other.Amount, other.Currency));

    public Money Multiply(decimal factor) => new(Amount * factor, Currency);
}
