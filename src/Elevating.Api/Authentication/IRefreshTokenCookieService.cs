using Elevating.Application.Common.Authentication;

namespace Elevating.Api.Authentication;

public interface IRefreshTokenCookieService
{
    string? Read(HttpRequest request);

    void Write(
        HttpResponse response,
        RefreshTokenResult refreshToken);

    void Clear(HttpResponse response);
}