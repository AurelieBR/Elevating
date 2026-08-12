using Elevating.Application.Common.Authentication;
using Elevating.Application.DTOs.Authentication;
using Elevating.Application.Interfaces.Authentication;
using Elevating.Application.Services;

using Moq;

namespace Elevating.Application.Tests.Services;

public sealed class AuthenticationServiceTests
{
    private readonly Mock<IIdentityService> identityServiceMock = new();
    private readonly Mock<IAccessTokenService> accessTokenServiceMock = new();

    [Fact]
    public async Task RegisterAsync_WhenIdentitySucceeds_ShouldIssueToken()
    {
        // Arrange
        var user = new AuthenticatedUser(
            Guid.NewGuid(),
            "user@example.com");

        var expiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(15);

        identityServiceMock
            .Setup(service => service.RegisterAsync(
                "user@example.com",
                "StrongPass1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserIdentityResult(
                AuthenticationStatus.Succeeded,
                user));

        accessTokenServiceMock
            .Setup(service => service.CreateAccessToken(user))
            .Returns(new AccessTokenResult(
                "access-token",
                expiresAtUtc));

        var service = CreateService();

        // Act
        var result = await service.RegisterAsync(
            new RegisterRequest(
                "  user@example.com  ",
                "StrongPass1"));

        // Assert
        Assert.Equal(AuthenticationStatus.Succeeded, result.Status);
        Assert.NotNull(result.Response);
        Assert.Equal(user.Id, result.Response.UserId);
        Assert.Equal(user.Email, result.Response.Email);
        Assert.Equal("access-token", result.Response.AccessToken);
        Assert.Equal(expiresAtUtc, result.Response.ExpiresAtUtc);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailIsDuplicate_ShouldNotIssueToken()
    {
        // Arrange
        identityServiceMock
            .Setup(service => service.RegisterAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserIdentityResult(
                AuthenticationStatus.DuplicateEmail));

        var service = CreateService();

        // Act
        var result = await service.RegisterAsync(
            new RegisterRequest(
                "user@example.com",
                "StrongPass1"));

        // Assert
        Assert.Equal(AuthenticationStatus.DuplicateEmail, result.Status);
        Assert.Null(result.Response);

        accessTokenServiceMock.Verify(
            tokenService => tokenService.CreateAccessToken(
                It.IsAny<AuthenticatedUser>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsAreValid_ShouldIssueToken()
    {
        // Arrange
        var user = new AuthenticatedUser(
            Guid.NewGuid(),
            "user@example.com");

        identityServiceMock
            .Setup(service => service.ValidateCredentialsAsync(
                "user@example.com",
                "StrongPass1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserIdentityResult(
                AuthenticationStatus.Succeeded,
                user));

        accessTokenServiceMock
            .Setup(service => service.CreateAccessToken(user))
            .Returns(new AccessTokenResult(
                "access-token",
                DateTimeOffset.UtcNow.AddMinutes(15)));

        var service = CreateService();

        // Act
        var result = await service.LoginAsync(
            new LoginRequest(
                "user@example.com",
                "StrongPass1"));

        // Assert
        Assert.Equal(AuthenticationStatus.Succeeded, result.Status);
        Assert.NotNull(result.Response);
        Assert.Equal("access-token", result.Response.AccessToken);
    }

    [Theory]
    [InlineData(AuthenticationStatus.InvalidCredentials)]
    [InlineData(AuthenticationStatus.LockedOut)]
    [InlineData(AuthenticationStatus.Failed)]
    public async Task LoginAsync_WhenIdentityRejectsCredentials_ShouldNotIssueToken(
        AuthenticationStatus status)
    {
        // Arrange
        identityServiceMock
            .Setup(service => service.ValidateCredentialsAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserIdentityResult(status));

        var service = CreateService();

        // Act
        var result = await service.LoginAsync(
            new LoginRequest(
                "user@example.com",
                "WrongPassword1"));

        // Assert
        Assert.Equal(status, result.Status);
        Assert.Null(result.Response);

        accessTokenServiceMock.Verify(
            tokenService => tokenService.CreateAccessToken(
                It.IsAny<AuthenticatedUser>()),
            Times.Never);
    }

    private AuthenticationService CreateService()
    {
        return new AuthenticationService(
            identityServiceMock.Object,
            accessTokenServiceMock.Object);
    }
}