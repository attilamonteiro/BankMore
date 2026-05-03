namespace BankMore.Shared.Authentication;

public sealed record JwtSettings
{
    public string Secret { get; init; } = string.Empty;
    public string Issuer { get; init; } = "BankMore";
    public string Audience { get; init; } = "BankMore";
    public int ExpirationMinutes { get; init; } = 60;
}
