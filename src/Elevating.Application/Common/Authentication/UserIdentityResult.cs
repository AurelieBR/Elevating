namespace Elevating.Application.Common.Authentication;

public sealed record UserIdentityResult(
    AuthenticationStatus Status,
    AuthenticatedUser? User = null);