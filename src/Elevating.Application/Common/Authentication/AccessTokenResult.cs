namespace Elevating.Application.Common.Authentication;

public sealed record AccessTokenResult(
    string Value,
    DateTimeOffset ExpiresAtUtc);