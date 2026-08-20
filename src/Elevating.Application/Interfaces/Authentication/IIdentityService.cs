using Elevating.Application.Common.Authentication;

namespace Elevating.Application.Interfaces.Authentication;

public interface IIdentityService
{
    Task<UserIdentityResult> RegisterAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<UserIdentityResult> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<AuthenticatedUser?> FindByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}