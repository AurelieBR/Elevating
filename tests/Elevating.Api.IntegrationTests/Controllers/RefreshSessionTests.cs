using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

using Elevating.Api.IntegrationTests.Infrastructure;
using Elevating.Application.DTOs.Authentication;
using Elevating.Infrastructure.Persistence;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Elevating.Api.IntegrationTests.Controllers;

public sealed class RefreshSessionTests
    : IClassFixture<ElevatingApiFactory>
{
    private const string CookieName =
        "__Secure-Elevating.RefreshToken";
    private const string ValidPassword = "StrongPass1";

    private readonly ElevatingApiFactory factory;
    private readonly HttpClient client;

    public RefreshSessionTests(ElevatingApiFactory factory)
    {
        this.factory = factory;

        client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = false
            });
    }

    [Fact]
    public async Task Register_ShouldSetSecureCookieAndPersistOnlyHash()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        // Act
        var session = await RegisterAsync(
            "register.session@example.com");

        // Assert
        AssertRefreshCookieSecurity(session.SetCookieHeader);
        Assert.DoesNotContain(
            session.RefreshToken,
            session.Json,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "refreshToken",
            session.Json,
            StringComparison.OrdinalIgnoreCase);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var storedToken = await dbContext.RefreshTokens.SingleAsync();
        var expectedHash = HashToken(session.RefreshToken);

        Assert.Equal(expectedHash, storedToken.TokenHash);
        Assert.NotEqual(session.RefreshToken, storedToken.TokenHash);
        Assert.Equal(64, storedToken.TokenHash.Length);
        Assert.Null(storedToken.RevokedAtUtc);
        Assert.InRange(
            storedToken.ExpiresAtUtc,
            DateTimeOffset.UtcNow.AddDays(6),
            DateTimeOffset.UtcNow.AddDays(8));
    }

    [Fact]
    public async Task Login_ShouldSetRefreshCookieWithoutReturningRawToken()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        await RegisterAsync("login.session@example.com");

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                "login.session@example.com",
                ValidPassword));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var setCookie = GetRefreshSetCookie(response);
        var refreshToken = ExtractRefreshToken(setCookie);

        AssertRefreshCookieSecurity(setCookie);
        Assert.DoesNotContain(
            refreshToken,
            json,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "refreshToken",
            json,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Refresh_WithValidToken_ShouldRotateAndIssueNewSession()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        var originalSession = await RegisterAsync(
            "rotate.session@example.com");

        // Act
        var response = await PostWithRefreshCookieAsync(
            "/api/auth/refresh",
            originalSession.RefreshToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var authentication = await response.Content
            .ReadFromJsonAsync<AuthenticationResponse>();

        Assert.NotNull(authentication);
        Assert.NotEqual(
            originalSession.Authentication.AccessToken,
            authentication.AccessToken);

        var replacementSetCookie = GetRefreshSetCookie(response);
        var replacementRawToken =
            ExtractRefreshToken(replacementSetCookie);

        AssertRefreshCookieSecurity(replacementSetCookie);
        Assert.NotEqual(
            originalSession.RefreshToken,
            replacementRawToken);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var tokens = await dbContext.RefreshTokens
            .AsNoTracking()
            .ToListAsync();

        Assert.Equal(2, tokens.Count);

        var originalToken = tokens.Single(token =>
            token.TokenHash == HashToken(
                originalSession.RefreshToken));

        var replacementToken = tokens.Single(token =>
            token.TokenHash == HashToken(replacementRawToken));

        Assert.NotNull(originalToken.RevokedAtUtc);
        Assert.Equal(
            replacementToken.Id,
            originalToken.ReplacedByTokenId);
        Assert.Null(replacementToken.RevokedAtUtc);
    }

    [Fact]
    public async Task Refresh_WhenOldTokenIsReused_ShouldReturnUnauthorized()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        var originalSession = await RegisterAsync(
            "reuse.session@example.com");

        var firstRefresh = await PostWithRefreshCookieAsync(
            "/api/auth/refresh",
            originalSession.RefreshToken);

        Assert.Equal(HttpStatusCode.OK, firstRefresh.StatusCode);

        // Act
        var reuseResponse = await PostWithRefreshCookieAsync(
            "/api/auth/refresh",
            originalSession.RefreshToken);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            reuseResponse.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithExpiredToken_ShouldReturnUnauthorizedAndClearCookie()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        var session = await RegisterAsync(
            "expired.session@example.com");

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var storedToken = await dbContext.RefreshTokens
                .SingleAsync(token =>
                    token.TokenHash == HashToken(
                        session.RefreshToken));

            storedToken.ExpiresAtUtc =
                DateTimeOffset.UtcNow.AddMinutes(-1);

            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await PostWithRefreshCookieAsync(
            "/api/auth/refresh",
            session.RefreshToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        AssertClearsRefreshCookie(response);
    }

    [Fact]
    public async Task Refresh_WithMalformedOrUnknownToken_ShouldReturnUnauthorized()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        var malformedResponse = await PostWithRefreshCookieAsync(
            "/api/auth/refresh",
            "not-a-refresh-token");

        // Act
        var unknownResponse = await PostWithRefreshCookieAsync(
            "/api/auth/refresh",
            new string('A', 128));

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            malformedResponse.StatusCode);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            unknownResponse.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithoutCookie_ShouldReturnUnauthorized()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        // Act
        var response = await client.PostAsync(
            "/api/auth/refresh",
            content: null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_ShouldRevokeTokenClearCookieAndPreventRefresh()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        var session = await RegisterAsync(
            "logout.session@example.com");

        // Act
        var logoutResponse = await PostWithRefreshCookieAsync(
            "/api/auth/logout",
            session.RefreshToken);

        // Assert
        Assert.Equal(
            HttpStatusCode.NoContent,
            logoutResponse.StatusCode);
        AssertClearsRefreshCookie(logoutResponse);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var storedToken = await dbContext.RefreshTokens
                .AsNoTracking()
                .SingleAsync(token =>
                    token.TokenHash == HashToken(
                        session.RefreshToken));

            Assert.NotNull(storedToken.RevokedAtUtc);
        }

        var refreshResponse = await PostWithRefreshCookieAsync(
            "/api/auth/refresh",
            session.RefreshToken);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            refreshResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_WithoutCookie_ShouldSucceedAndClearCookie()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        // Act
        var response = await client.PostAsync(
            "/api/auth/logout",
            content: null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        AssertClearsRefreshCookie(response);
    }

    [Fact]
    public async Task Me_WithValidAccessToken_ShouldReturnSafeCurrentUser()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        var session = await RegisterAsync("me@example.com");

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/auth/me");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                session.Authentication.AccessToken);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var currentUser = await response.Content
            .ReadFromJsonAsync<CurrentUserResponse>();

        Assert.NotNull(currentUser);
        Assert.Equal(
            session.Authentication.UserId,
            currentUser.UserId);
        Assert.Equal(
            session.Authentication.Email,
            currentUser.Email);

        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(
            "passwordHash",
            json,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "securityStamp",
            json,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "refreshToken",
            json,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Me_WithoutOrWithInvalidAccessToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var noTokenResponse = await client.GetAsync("/api/auth/me");

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/auth/me");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                "invalid-access-token");

        // Act
        var invalidTokenResponse = await client.SendAsync(request);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            noTokenResponse.StatusCode);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            invalidTokenResponse.StatusCode);
    }

    private async Task<SessionResponse> RegisterAsync(string email)
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(email, ValidPassword));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var authentication = await response.Content
            .ReadFromJsonAsync<AuthenticationResponse>();

        var setCookie = GetRefreshSetCookie(response);

        return new SessionResponse(
            Assert.IsType<AuthenticationResponse>(authentication),
            ExtractRefreshToken(setCookie),
            setCookie,
            json);
    }

    private async Task<HttpResponseMessage> PostWithRefreshCookieAsync(
        string path,
        string refreshToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            path);

        request.Headers.Add(
            "Cookie",
            $"{CookieName}={refreshToken}");

        return await client.SendAsync(request);
    }

    private static string GetRefreshSetCookie(
        HttpResponseMessage response)
    {
        return response.Headers
            .GetValues("Set-Cookie")
            .Single(header => header.StartsWith(
                $"{CookieName}=",
                StringComparison.Ordinal));
    }

    private static string ExtractRefreshToken(string setCookie)
    {
        var prefix = $"{CookieName}=";
        var endIndex = setCookie.IndexOf(';');

        return setCookie[prefix.Length..endIndex];
    }

    private static void AssertRefreshCookieSecurity(string setCookie)
    {
        Assert.Contains(
            "httponly",
            setCookie,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "secure",
            setCookie,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "samesite=none",
            setCookie,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "path=/api/auth",
            setCookie,
            StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertClearsRefreshCookie(
        HttpResponseMessage response)
    {
        var setCookie = GetRefreshSetCookie(response);

        Assert.Contains(
            $"{CookieName}=;",
            setCookie,
            StringComparison.Ordinal);
        Assert.Contains(
            "expires=",
            setCookie,
            StringComparison.OrdinalIgnoreCase);
    }

    private static string HashToken(string refreshToken)
    {
        return Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(refreshToken)));
    }

    private sealed record SessionResponse(
        AuthenticationResponse Authentication,
        string RefreshToken,
        string SetCookieHeader,
        string Json);
}