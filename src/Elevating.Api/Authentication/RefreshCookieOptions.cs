namespace Elevating.Api.Authentication;

public sealed class RefreshCookieOptions
{
    public const string SectionName = "RefreshCookie";

    public string Name { get; set; } =
        "__Secure-Elevating.RefreshToken";

    public SameSiteMode SameSite { get; set; } = SameSiteMode.None;

    public string Path { get; set; } = "/api/auth";
}