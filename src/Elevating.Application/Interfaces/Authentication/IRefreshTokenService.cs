using Elevating.Application.Common.Authentication;

namespace Elevating.Application.Interfaces.Authentication;

public interface IRefreshTokenService
{
    Task<RefreshTokenResult> CreateAsync(
        AuthenticatedUser user,
        CancellationToken cancellationToken = default);

    Task<RefreshSessionResult> RotateAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task RevokeAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);
}