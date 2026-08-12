using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;

using Elevating.Api.IntegrationTests.Infrastructure;
using Elevating.Application.DTOs.Authentication;
using Elevating.Infrastructure.Identity;
using Elevating.Infrastructure.Persistence;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Elevating.Api.IntegrationTests.Controllers;

public sealed class AuthControllerTests
    : IClassFixture<ElevatingApiFactory>
{
    private const string ValidPassword = "StrongPass1";

    private readonly ElevatingApiFactory factory;
    private readonly HttpClient client;

    public AuthControllerTests(ElevatingApiFactory factory)
    {
        this.factory = factory;

        client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
    }

    [Fact]
    public async Task Register_WithValidRequest_ShouldPersistHashedUserAndIssueJwt()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        var beforeRegistration = DateTimeOffset.UtcNow;

        var request = new RegisterRequest(
            "new.user@example.com",
            ValidPassword);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            request);

        var afterRegistration = DateTimeOffset.UtcNow;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var authentication = await response.Content
            .ReadFromJsonAsync<AuthenticationResponse>();

        Assert.NotNull(authentication);
        Assert.NotEqual(Guid.Empty, authentication.UserId);
        Assert.Equal(request.Email, authentication.Email);
        Assert.False(string.IsNullOrWhiteSpace(
            authentication.AccessToken));
        Assert.InRange(
            authentication.ExpiresAtUtc,
            beforeRegistration.AddMinutes(14),
            afterRegistration.AddMinutes(16));

        using var scope = factory.Services.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.FindByEmailAsync(request.Email);

        Assert.NotNull(user);
        Assert.Equal(authentication.UserId, user.Id);
        Assert.NotNull(user.PasswordHash);
        Assert.NotEqual(request.Password, user.PasswordHash);

        var passwordVerification = userManager.PasswordHasher
            .VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

        Assert.NotEqual(
            PasswordVerificationResult.Failed,
            passwordVerification);

        var jwt = new JwtSecurityTokenHandler()
            .ReadJwtToken(authentication.AccessToken);

        Assert.Equal(SecurityAlgorithms.RsaSha256, jwt.Header.Alg);
        Assert.Equal(
            authentication.UserId.ToString(),
            jwt.Claims.Single(claim =>
                claim.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(
            request.Email,
            jwt.Claims.Single(claim =>
                claim.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.NotEmpty(jwt.Claims.Single(claim =>
            claim.Type == JwtRegisteredClaimNames.Jti).Value);
        Assert.NotNull(jwt.Claims.SingleOrDefault(claim =>
            claim.Type == JwtRegisteredClaimNames.Iat));
        Assert.NotNull(jwt.Claims.SingleOrDefault(claim =>
            claim.Type == JwtRegisteredClaimNames.Exp));
    }

    [Fact]
    public async Task Register_WhenEmailAlreadyExists_ShouldReturnConflict()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        var request = new RegisterRequest(
            "duplicate@example.com",
            ValidPassword);

        var firstResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            request);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        // Act
        var secondResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        Assert.Equal(1, await dbContext.Users.CountAsync());
    }

    [Fact]
    public async Task Register_WithInvalidPassword_ShouldReturnBadRequestWithoutUser()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        var request = new RegisterRequest(
            "invalid.password@example.com",
            "weak");

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content
            .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Contains("Password", problem.Errors.Keys);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        Assert.False(await dbContext.Users.AnyAsync());
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_ShouldReturnAccessToken()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        var registration = await RegisterUserAsync(
            "login.success@example.com");

        var request = new LoginRequest(
            registration.Email,
            ValidPassword);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var authentication = await response.Content
            .ReadFromJsonAsync<AuthenticationResponse>();

        Assert.NotNull(authentication);
        Assert.Equal(registration.UserId, authentication.UserId);
        Assert.False(string.IsNullOrWhiteSpace(
            authentication.AccessToken));
    }

    [Fact]
    public async Task Login_WithWrongPasswordOrUnknownEmail_ShouldReturnEquivalentUnauthorizedResponse()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        await RegisterUserAsync("known.user@example.com");

        // Act
        var wrongPasswordResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                "known.user@example.com",
                "WrongPass1"));

        var unknownEmailResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                "unknown.user@example.com",
                "WrongPass1"));

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            wrongPasswordResponse.StatusCode);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            unknownEmailResponse.StatusCode);

        var wrongPasswordProblem = await wrongPasswordResponse.Content
            .ReadFromJsonAsync<ProblemDetails>();

        var unknownEmailProblem = await unknownEmailResponse.Content
            .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(wrongPasswordProblem);
        Assert.NotNull(unknownEmailProblem);
        Assert.Equal(
            wrongPasswordProblem.Title,
            unknownEmailProblem.Title);
        Assert.Equal(
            wrongPasswordProblem.Detail,
            unknownEmailProblem.Detail);
        Assert.Equal(
            wrongPasswordProblem.Status,
            unknownEmailProblem.Status);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithValidIssuedJwt_ShouldReturnNoContent()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        var registration = await RegisterUserAsync(
            "valid.jwt@example.com");

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/test-auth/protected");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                registration.AccessToken);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithMalformedJwt_ShouldReturnUnauthorized()
    {
        // Arrange
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/test-auth/protected");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                "not-a-valid-jwt");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithExpiredJwt_ShouldReturnUnauthorized()
    {
        // Arrange
        var expiredToken = CreateExpiredToken();

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/test-auth/protected");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                expiredToken);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_AfterRepeatedFailures_ShouldLockOutUser()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        const string email = "lockout@example.com";
        await RegisterUserAsync(email);

        // Act
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var failedResponse = await client.PostAsJsonAsync(
                "/api/auth/login",
                new LoginRequest(email, "WrongPass1"));

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                failedResponse.StatusCode);
        }

        // Assert
        using var scope = factory.Services.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.FindByEmailAsync(email);

        Assert.NotNull(user);
        Assert.True(await userManager.IsLockedOutAsync(user));
        Assert.True(user.LockoutEnd > DateTimeOffset.UtcNow);

        var correctPasswordResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, ValidPassword));

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            correctPasswordResponse.StatusCode);
    }

    private async Task<AuthenticationResponse> RegisterUserAsync(
        string email)
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(email, ValidPassword));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var authentication = await response.Content
            .ReadFromJsonAsync<AuthenticationResponse>();

        return Assert.IsType<AuthenticationResponse>(authentication);
    }

    private string CreateExpiredToken()
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(factory.JwtPrivateKeyPem);

        var credentials = new SigningCredentials(
            new RsaSecurityKey(
                rsa.ExportParameters(includePrivateParameters: true)),
            SecurityAlgorithms.RsaSha256);

        var now = DateTime.UtcNow;

        var token = new JwtSecurityToken(
            issuer: ElevatingApiFactory.JwtIssuer,
            audience: ElevatingApiFactory.JwtAudience,
            claims:
            [
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    Guid.NewGuid().ToString()),
                new Claim(
                    JwtRegisteredClaimNames.Email,
                    "expired.jwt@example.com")
            ],
            notBefore: now.AddMinutes(-5),
            expires: now.AddMinutes(-1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}