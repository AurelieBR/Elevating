using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

using Elevating.Application.Common.Authentication;
using Elevating.Application.Interfaces.Authentication;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Elevating.Infrastructure.Authentication;

public sealed class JwtAccessTokenService(
    IOptions<JwtOptions> options,
    TimeProvider timeProvider)
    : IAccessTokenService
{
    private readonly JwtOptions jwtOptions = options.Value;

    public AccessTokenResult CreateAccessToken(AuthenticatedUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var now = timeProvider.GetUtcNow();
        var expiresAtUtc = now.AddMinutes(
            jwtOptions.AccessTokenMinutes);

        var claims = new[]
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),
            new Claim(
                JwtRegisteredClaimNames.Email,
                user.Email),
            new Claim(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),
            new Claim(
                JwtRegisteredClaimNames.Iat,
                now.ToUnixTimeSeconds().ToString(
                    CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64)
        };

        using var rsa = RSA.Create();
        rsa.ImportFromPem(jwtOptions.PrivateKeyPem);

        var signingKey = new RsaSecurityKey(
            rsa.ExportParameters(includePrivateParameters: true));

        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: credentials);

        return new AccessTokenResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAtUtc);
    }
}