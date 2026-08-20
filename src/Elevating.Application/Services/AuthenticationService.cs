using Elevating.Application.Common.Authentication;
using Elevating.Application.DTOs.Authentication;
using Elevating.Application.Interfaces.Authentication;
using Elevating.Application.Interfaces.Services;

namespace Elevating.Application.Services;

public sealed class AuthenticationService(
    IIdentityService identityService,
    IAccessTokenService accessTokenService,
    IRefreshTokenService refreshTokenService)
    : IAuthenticationService
{
    public async Task<AuthenticationResult> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var identityResult = await identityService.RegisterAsync(
            request.Email.Trim(),
            request.Password,
            cancellationToken);

        return await CreateSessionAsync(
            identityResult,
            cancellationToken);
    }

    public async Task<AuthenticationResult> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var identityResult =
            await identityService.ValidateCredentialsAsync(
                request.Email.Trim(),
                request.Password,
                cancellationToken);

        return await CreateSessionAsync(
            identityResult,
            cancellationToken);
    }

    public async Task<AuthenticationResult> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return new AuthenticationResult(
                AuthenticationStatus.InvalidCredentials);
        }

        var refreshResult = await refreshTokenService.RotateAsync(
            refreshToken,
            cancellationToken);

        if (refreshResult.Status != AuthenticationStatus.Succeeded ||
            refreshResult.User is null ||
            refreshResult.RefreshToken is null)
        {
            return new AuthenticationResult(
                AuthenticationStatus.InvalidCredentials);
        }

        return CreateAuthenticationResult(
            refreshResult.User,
            refreshResult.RefreshToken);
    }

    public async Task LogoutAsync(
        string? refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        await refreshTokenService.RevokeAsync(
            refreshToken,
            cancellationToken);
    }

    public async Task<CurrentUserResponse?> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await identityService.FindByIdAsync(
            userId,
            cancellationToken);

        return user is null
            ? null
            : new CurrentUserResponse(user.Id, user.Email);
    }

    private async Task<AuthenticationResult> CreateSessionAsync(
        UserIdentityResult identityResult,
        CancellationToken cancellationToken)
    {
        if (identityResult.Status != AuthenticationStatus.Succeeded ||
            identityResult.User is null)
        {
            return new AuthenticationResult(identityResult.Status);
        }

        var refreshToken = await refreshTokenService.CreateAsync(
            identityResult.User,
            cancellationToken);

        return CreateAuthenticationResult(
            identityResult.User,
            refreshToken);
    }

    private AuthenticationResult CreateAuthenticationResult(
        AuthenticatedUser user,
        RefreshTokenResult refreshToken)
    {
        var accessToken = accessTokenService.CreateAccessToken(user);

        var response = new AuthenticationResponse(
            user.Id,
            user.Email,
            accessToken.Value,
            accessToken.ExpiresAtUtc);

        return new AuthenticationResult(
            AuthenticationStatus.Succeeded,
            response,
            refreshToken);
    }
}