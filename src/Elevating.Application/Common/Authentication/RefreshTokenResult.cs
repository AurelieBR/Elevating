namespace Elevating.Application.Common.Authentication;

public sealed record RefreshTokenResult(
    string Value,
    DateTimeOffset ExpiresAtUtc);