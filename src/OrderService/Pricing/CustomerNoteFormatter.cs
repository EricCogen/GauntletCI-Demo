namespace OrderService.Pricing;

public sealed class CustomerNoteFormatter
{
    public string Format(string? note)
    {
        return note!.Trim().ToUpperInvariant();
    }
}
