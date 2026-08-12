using Elevating.Application.Common.Authentication;

namespace Elevating.Application.Interfaces.Authentication;

public interface IAccessTokenService
{
    AccessTokenResult CreateAccessToken(AuthenticatedUser user);
}