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
    private readonly Mock<IRefreshTokenService> refreshTokenServiceMock = new();

    [Fact]
    public async Task RegisterAsync_WhenIdentitySucceeds_ShouldIssueSession()
    {
        // Arrange
        var user = CreateUser();
        var expiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(15);
        var refreshExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(7);

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

        refreshTokenServiceMock
            .Setup(service => service.CreateAsync(
                user,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshTokenResult(
                "refresh-token",
                refreshExpiresAtUtc));

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
        Assert.NotNull(result.RefreshToken);
        Assert.Equal("refresh-token", result.RefreshToken.Value);
        Assert.Equal(
            refreshExpiresAtUtc,
            result.RefreshToken.ExpiresAtUtc);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailIsDuplicate_ShouldNotIssueSession()
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
        Assert.Null(result.RefreshToken);

        VerifyNoSessionIssued();
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsAreValid_ShouldIssueSession()
    {
        // Arrange
        var user = CreateUser();
        SetupSuccessfulSession(user);

        identityServiceMock
            .Setup(service => service.ValidateCredentialsAsync(
                "user@example.com",
                "StrongPass1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserIdentityResult(
                AuthenticationStatus.Succeeded,
                user));

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
        Assert.NotNull(result.RefreshToken);
        Assert.Equal("refresh-token", result.RefreshToken.Value);
    }

    [Theory]
    [InlineData(AuthenticationStatus.InvalidCredentials)]
    [InlineData(AuthenticationStatus.LockedOut)]
    [InlineData(AuthenticationStatus.Failed)]
    public async Task LoginAsync_WhenIdentityRejectsCredentials_ShouldNotIssueSession(
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
        Assert.Null(result.RefreshToken);

        VerifyNoSessionIssued();
    }

    [Fact]
    public async Task RefreshAsync_WhenRotationSucceeds_ShouldIssueAccessToken()
    {
        // Arrange
        var user = CreateUser();
        var replacementToken = new RefreshTokenResult(
            "replacement-token",
            DateTimeOffset.UtcNow.AddDays(7));

        refreshTokenServiceMock
            .Setup(service => service.RotateAsync(
                "current-token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshSessionResult(
                AuthenticationStatus.Succeeded,
                user,
                replacementToken));

        accessTokenServiceMock
            .Setup(service => service.CreateAccessToken(user))
            .Returns(new AccessTokenResult(
                "fresh-access-token",
                DateTimeOffset.UtcNow.AddMinutes(15)));

        var service = CreateService();

        // Act
        var result = await service.RefreshAsync("current-token");

        // Assert
        Assert.Equal(AuthenticationStatus.Succeeded, result.Status);
        Assert.NotNull(result.Response);
        Assert.Equal(
            "fresh-access-token",
            result.Response.AccessToken);
        Assert.Equal(replacementToken, result.RefreshToken);
    }

    [Fact]
    public async Task RefreshAsync_WhenRotationFails_ShouldNotIssueAccessToken()
    {
        // Arrange
        refreshTokenServiceMock
            .Setup(service => service.RotateAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshSessionResult(
                AuthenticationStatus.InvalidCredentials));

        var service = CreateService();

        // Act
        var result = await service.RefreshAsync("invalid-token");

        // Assert
        Assert.Equal(
            AuthenticationStatus.InvalidCredentials,
            result.Status);
        Assert.Null(result.Response);
        Assert.Null(result.RefreshToken);

        accessTokenServiceMock.Verify(
            service => service.CreateAccessToken(
                It.IsAny<AuthenticatedUser>()),
            Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_WithToken_ShouldRevokeIt()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.LogoutAsync("refresh-token");

        // Assert
        refreshTokenServiceMock.Verify(
            refreshService => refreshService.RevokeAsync(
                "refresh-token",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_WithoutToken_ShouldSucceedWithoutRevocation()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.LogoutAsync(null);

        // Assert
        refreshTokenServiceMock.Verify(
            refreshService => refreshService.RevokeAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenUserExists_ShouldReturnSafeResponse()
    {
        // Arrange
        var user = CreateUser();

        identityServiceMock
            .Setup(service => service.FindByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var service = CreateService();

        // Act
        var result = await service.GetCurrentUserAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(user.Email, result.Email);
    }

    private AuthenticationService CreateService()
    {
        return new AuthenticationService(
            identityServiceMock.Object,
            accessTokenServiceMock.Object,
            refreshTokenServiceMock.Object);
    }

    private void SetupSuccessfulSession(AuthenticatedUser user)
    {
        accessTokenServiceMock
            .Setup(service => service.CreateAccessToken(user))
            .Returns(new AccessTokenResult(
                "access-token",
                DateTimeOffset.UtcNow.AddMinutes(15)));

        refreshTokenServiceMock
            .Setup(service => service.CreateAsync(
                user,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshTokenResult(
                "refresh-token",
                DateTimeOffset.UtcNow.AddDays(7)));
    }

    private void VerifyNoSessionIssued()
    {
        accessTokenServiceMock.Verify(
            service => service.CreateAccessToken(
                It.IsAny<AuthenticatedUser>()),
            Times.Never);

        refreshTokenServiceMock.Verify(
            service => service.CreateAsync(
                It.IsAny<AuthenticatedUser>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static AuthenticatedUser CreateUser()
    {
        return new AuthenticatedUser(
            Guid.NewGuid(),
            "user@example.com");
    }
}