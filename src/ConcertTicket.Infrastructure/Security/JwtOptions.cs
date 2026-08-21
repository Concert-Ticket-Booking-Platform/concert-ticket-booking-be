namespace ConcertTicket.Infrastructure.Security;

public sealed class JwtOptions
{
    public string Secret { get; set; } = null!;

    public string Issuer { get; set; } = null!;

    public string Audience { get; set; } = null!;

    public int ExpiresMinutes { get; set; }
}
