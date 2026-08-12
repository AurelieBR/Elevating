using System.IdentityModel.Tokens.Jwt;

using Elevating.Application.Interfaces.Authentication;

namespace Elevating.Api.Authentication;

public sealed class HttpCurrentUser(
    IHttpContextAccessor httpContextAccessor)
    : ICurrentUser
{
    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?
            .User.Identity?.IsAuthenticated == true;

    public Guid? UserId
    {
        get
        {
            if (!IsAuthenticated)
            {
                return null;
            }

            var subject = httpContextAccessor.HttpContext?
                .User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            return Guid.TryParse(subject, out var userId)
                ? userId
                : null;
        }
    }
}