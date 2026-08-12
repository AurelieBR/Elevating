using Elevating.Application.Common.Authentication;

using Microsoft.Extensions.Options;

namespace Elevating.Api.Authentication;

public sealed class RefreshTokenCookieService(
    IOptions<RefreshCookieOptions> options,
    TimeProvider timeProvider)
    : IRefreshTokenCookieService
{
    private readonly RefreshCookieOptions cookieOptions =
        options.Value;

    public string? Read(HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.Cookies.TryGetValue(
            cookieOptions.Name,
            out var refreshToken)
            ? refreshToken
            : null;
    }

    public void Write(
        HttpResponse response,
        RefreshTokenResult refreshToken)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(refreshToken);

        var maxAge = refreshToken.ExpiresAtUtc -
            timeProvider.GetUtcNow();

        response.Cookies.Append(
            cookieOptions.Name,
            refreshToken.Value,
            CreateCookieOptions(
                refreshToken.ExpiresAtUtc,
                maxAge));
    }

    public void Clear(HttpResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        response.Cookies.Delete(
            cookieOptions.Name,
            CreateCookieOptions(
                DateTimeOffset.UnixEpoch,
                TimeSpan.Zero));
    }

    private CookieOptions CreateCookieOptions(
        DateTimeOffset expiresAtUtc,
        TimeSpan maxAge)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = cookieOptions.SameSite,
            Path = cookieOptions.Path,
            IsEssential = true,
            Expires = expiresAtUtc,
            MaxAge = maxAge
        };
    }
}