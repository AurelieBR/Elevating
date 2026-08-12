namespace Elevating.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;

    public string PrivateKeyPem { get; set; } = string.Empty;

    public string PublicKeyPem { get; set; } = string.Empty;
}