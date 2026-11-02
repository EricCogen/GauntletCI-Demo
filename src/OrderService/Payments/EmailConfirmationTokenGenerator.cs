namespace OrderService.Payments;

public sealed class EmailConfirmationTokenGenerator
{
    public string NewConfirmationToken()
    {
        var rng = new Random();
        var token = new char[16];
        for (var i = 0; i < token.Length; i++)
        {
            token[i] = (char)('a' + rng.Next(26));
        }
        return new string(token);
    }
}
