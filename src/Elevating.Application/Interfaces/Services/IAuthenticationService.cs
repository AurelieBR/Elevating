using Elevating.Application.Common.Authentication;
using Elevating.Application.DTOs.Authentication;

namespace Elevating.Application.Interfaces.Services;

public interface IAuthenticationService
{
    Task<AuthenticationResult> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthenticationResult> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthenticationResult> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(
        string? refreshToken,
        CancellationToken cancellationToken = default);

    Task<CurrentUserResponse?> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}