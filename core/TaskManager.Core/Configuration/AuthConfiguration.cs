namespace TaskManager.Core.Configuration;

public sealed class AuthConfiguration
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public double ExpirationInHours { get; set; } = 0.25;
    public int RefreshTokenExpirationInDays { get; set; } = 7;
}
