using Elevating.Application.DTOs.Authentication;

namespace Elevating.Application.Common.Authentication;

public sealed record AuthenticationResult(
    AuthenticationStatus Status,
    AuthenticationResponse? Response = null,
    RefreshTokenResult? RefreshToken = null);