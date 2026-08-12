using Elevating.Application.Common.Authentication;
using Elevating.Application.Interfaces.Authentication;

using Microsoft.AspNetCore.Identity;

namespace Elevating.Infrastructure.Identity;

public sealed class IdentityService(
    UserManager<ApplicationUser> userManager)
    : IIdentityService
{
    public async Task<UserIdentityResult> RegisterAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            LockoutEnabled = true
        };

        var result = await userManager.CreateAsync(user, password);

        cancellationToken.ThrowIfCancellationRequested();

        if (result.Succeeded)
        {
            return new UserIdentityResult(
                AuthenticationStatus.Succeeded,
                new AuthenticatedUser(user.Id, user.Email!));
        }

        var duplicateEmail = result.Errors.Any(error =>
            error.Code == nameof(IdentityErrorDescriber.DuplicateEmail) ||
            error.Code == nameof(IdentityErrorDescriber.DuplicateUserName));

        return new UserIdentityResult(
            duplicateEmail
                ? AuthenticationStatus.DuplicateEmail
                : AuthenticationStatus.Failed);
    }

    public async Task<UserIdentityResult> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return new UserIdentityResult(
                AuthenticationStatus.InvalidCredentials);
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return new UserIdentityResult(
                AuthenticationStatus.LockedOut);
        }

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            var failureResult = await userManager.AccessFailedAsync(user);

            if (!failureResult.Succeeded)
            {
                return new UserIdentityResult(
                    AuthenticationStatus.Failed);
            }

            return new UserIdentityResult(
                await userManager.IsLockedOutAsync(user)
                    ? AuthenticationStatus.LockedOut
                    : AuthenticationStatus.InvalidCredentials);
        }

        if (await userManager.GetAccessFailedCountAsync(user) > 0)
        {
            var resetResult =
                await userManager.ResetAccessFailedCountAsync(user);

            if (!resetResult.Succeeded)
            {
                return new UserIdentityResult(
                    AuthenticationStatus.Failed);
            }
        }

        cancellationToken.ThrowIfCancellationRequested();

        return string.IsNullOrWhiteSpace(user.Email)
            ? new UserIdentityResult(AuthenticationStatus.Failed)
            : new UserIdentityResult(
                AuthenticationStatus.Succeeded,
                new AuthenticatedUser(user.Id, user.Email));
    }
}