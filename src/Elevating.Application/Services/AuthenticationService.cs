using Elevating.Application.Common.Authentication;
using Elevating.Application.DTOs.Authentication;
using Elevating.Application.Interfaces.Authentication;
using Elevating.Application.Interfaces.Services;

namespace Elevating.Application.Services;

public sealed class AuthenticationService(
    IIdentityService identityService,
    IAccessTokenService accessTokenService)
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

        return CreateAuthenticationResult(identityResult);
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

        return CreateAuthenticationResult(identityResult);
    }

    private AuthenticationResult CreateAuthenticationResult(
        UserIdentityResult identityResult)
    {
        if (identityResult.Status != AuthenticationStatus.Succeeded ||
            identityResult.User is null)
        {
            return new AuthenticationResult(identityResult.Status);
        }

        var accessToken = accessTokenService.CreateAccessToken(
            identityResult.User);

        var response = new AuthenticationResponse(
            identityResult.User.Id,
            identityResult.User.Email,
            accessToken.Value,
            accessToken.ExpiresAtUtc);

        return new AuthenticationResult(
            AuthenticationStatus.Succeeded,
            response);
    }
}