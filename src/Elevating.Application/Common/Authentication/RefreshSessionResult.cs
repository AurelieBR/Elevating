namespace Elevating.Application.Common.Authentication;

public sealed record RefreshSessionResult(
    AuthenticationStatus Status,
    AuthenticatedUser? User = null,
    RefreshTokenResult? RefreshToken = null);