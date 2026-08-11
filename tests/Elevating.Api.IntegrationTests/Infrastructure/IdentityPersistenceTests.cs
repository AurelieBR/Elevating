using Elevating.Infrastructure.Identity;
using Elevating.Infrastructure.Persistence;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Elevating.Api.IntegrationTests.Infrastructure;

public sealed class IdentityPersistenceTests
    : IClassFixture<ElevatingApiFactory>
{
    private readonly ElevatingApiFactory factory;

    public IdentityPersistenceTests(ElevatingApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task IdentityServices_ShouldPersistApplicationUserWithGuidKey()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        using var scope = factory.Services.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var user = new ApplicationUser
        {
            UserName = "identity.persistence@example.com",
            Email = "identity.persistence@example.com"
        };

        // Act
        var result = await userManager.CreateAsync(user);

        // Assert
        Assert.True(
            result.Succeeded,
            string.Join(", ", result.Errors.Select(error => error.Description)));

        Assert.NotEqual(Guid.Empty, user.Id);

        var storedUser = await dbContext.Users
            .AsNoTracking()
            .SingleAsync(candidate => candidate.Id == user.Id);

        Assert.Equal(user.UserName, storedUser.UserName);
        Assert.Equal(user.Email, storedUser.Email);
    }
}