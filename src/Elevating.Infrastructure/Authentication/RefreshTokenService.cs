using System.Security.Cryptography;
using System.Text;

using Elevating.Application.Common.Authentication;
using Elevating.Application.Interfaces.Authentication;
using Elevating.Infrastructure.Identity;
using Elevating.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Elevating.Infrastructure.Authentication;

public sealed class RefreshTokenService(
    AppDbContext dbContext,
    IOptions<RefreshTokenOptions> options,
    TimeProvider timeProvider)
    : IRefreshTokenService
{
    private const int RawTokenByteCount = 64;
    private const int RawTokenLength = RawTokenByteCount * 2;

    private readonly RefreshTokenOptions refreshTokenOptions =
        options.Value;

    public async Task<RefreshTokenResult> CreateAsync(
        AuthenticatedUser user,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        var now = timeProvider.GetUtcNow();
        var rawToken = GenerateRawToken();

        var refreshToken = CreateRefreshToken(
            user.Id,
            rawToken,
            now);

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResult(
            rawToken,
            refreshToken.ExpiresAtUtc);
    }

    public async Task<RefreshSessionResult> RotateAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (!HasValidFormat(refreshToken))
        {
            return InvalidSession();
        }

        var tokenHash = HashToken(refreshToken);

        var storedToken = await dbContext.RefreshTokens
            .Include(token => token.User)
            .SingleOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                cancellationToken);

        var now = timeProvider.GetUtcNow();

        if (storedToken is null ||
            storedToken.RevokedAtUtc.HasValue ||
            storedToken.ExpiresAtUtc <= now ||
            string.IsNullOrWhiteSpace(storedToken.User.Email) ||
            IsLockedOut(storedToken.User, now))
        {
            return InvalidSession();
        }

        var replacementRawToken = GenerateRawToken();
        var replacementToken = CreateRefreshToken(
            storedToken.UserId,
            replacementRawToken,
            now);

        storedToken.RevokedAtUtc = now;
        storedToken.ReplacedByTokenId = replacementToken.Id;

        dbContext.RefreshTokens.Add(replacementToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            dbContext.ChangeTracker.Clear();
            return InvalidSession();
        }

        return new RefreshSessionResult(
            AuthenticationStatus.Succeeded,
            new AuthenticatedUser(
                storedToken.User.Id,
                storedToken.User.Email),
            new RefreshTokenResult(
                replacementRawToken,
                replacementToken.ExpiresAtUtc));
    }

    public async Task RevokeAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (!HasValidFormat(refreshToken))
        {
            return;
        }

        var tokenHash = HashToken(refreshToken);

        var storedToken = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                cancellationToken);

        if (storedToken is null || storedToken.RevokedAtUtc.HasValue)
        {
            return;
        }

        storedToken.RevokedAtUtc = timeProvider.GetUtcNow();

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            dbContext.ChangeTracker.Clear();
        }
    }

    private RefreshToken CreateRefreshToken(
        Guid userId,
        string rawToken,
        DateTimeOffset now)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = HashToken(rawToken),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddDays(
                refreshTokenOptions.LifetimeDays)
        };
    }

    private static string GenerateRawToken()
    {
        return Convert.ToHexString(
            RandomNumberGenerator.GetBytes(RawTokenByteCount));
    }

    private static string HashToken(string rawToken)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(rawToken);
        return Convert.ToHexString(SHA256.HashData(tokenBytes));
    }

    private static bool HasValidFormat(string refreshToken)
    {
        return refreshToken.Length == RawTokenLength &&
            refreshToken.All(Uri.IsHexDigit);
    }

    private static bool IsLockedOut(
        ApplicationUser user,
        DateTimeOffset now)
    {
        return user.LockoutEnabled &&
            user.LockoutEnd.HasValue &&
            user.LockoutEnd.Value > now;
    }

    private static RefreshSessionResult InvalidSession()
    {
        return new RefreshSessionResult(
            AuthenticationStatus.InvalidCredentials);
    }
}