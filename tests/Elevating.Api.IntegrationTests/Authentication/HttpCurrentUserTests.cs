using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Elevating.Api.Authentication;

using Microsoft.AspNetCore.Http;

namespace Elevating.Api.IntegrationTests.Authentication;

public sealed class HttpCurrentUserTests
{
    [Fact]
    public void AuthenticatedPrincipal_WithGuidSubject_ShouldResolveUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentUser = CreateCurrentUser(
            new ClaimsIdentity(
                [
                    new Claim(
                        JwtRegisteredClaimNames.Sub,
                        userId.ToString())
                ],
                authenticationType: "Test"));

        // Act and assert
        Assert.True(currentUser.IsAuthenticated);
        Assert.Equal(userId, currentUser.UserId);
    }

    [Fact]
    public void UnauthenticatedPrincipal_ShouldNotResolveUserId()
    {
        // Arrange
        var currentUser = CreateCurrentUser(
            new ClaimsIdentity(
                [
                    new Claim(
                        JwtRegisteredClaimNames.Sub,
                        Guid.NewGuid().ToString())
                ]));

        // Act and assert
        Assert.False(currentUser.IsAuthenticated);
        Assert.Null(currentUser.UserId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-guid")]
    public void AuthenticatedPrincipal_WithInvalidSubject_ShouldNotResolveUserId(
        string? subject)
    {
        // Arrange
        var claims = subject is null
            ? Array.Empty<Claim>()
            :
            [
                new Claim(JwtRegisteredClaimNames.Sub, subject)
            ];

        var currentUser = CreateCurrentUser(
            new ClaimsIdentity(
                claims,
                authenticationType: "Test"));

        // Act and assert
        Assert.True(currentUser.IsAuthenticated);
        Assert.Null(currentUser.UserId);
    }

    private static HttpCurrentUser CreateCurrentUser(
        ClaimsIdentity identity)
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };

        return new HttpCurrentUser(
            new HttpContextAccessor
            {
                HttpContext = context
            });
    }
}