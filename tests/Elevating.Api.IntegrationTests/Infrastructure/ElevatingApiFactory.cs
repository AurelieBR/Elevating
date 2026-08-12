using System.Security.Cryptography;

using Elevating.Api.IntegrationTests.Controllers;
using Elevating.Domain.Entities;
using Elevating.Domain.Enums;
using Elevating.Infrastructure.Persistence;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Elevating.Api.IntegrationTests.Infrastructure;

public sealed class ElevatingApiFactory
    : WebApplicationFactory<Program>,
      IAsyncLifetime
{
    public const string JwtIssuer = "Elevating.Api.Tests";
    public const string JwtAudience = "Elevating.Web.Tests";

    private readonly SqliteConnection connection =
        new("Data Source=:memory:");

    public ElevatingApiFactory()
    {
        using var rsa = RSA.Create(2048);

        JwtPrivateKeyPem = rsa.ExportPkcs8PrivateKeyPem();
        JwtPublicKeyPem = rsa.ExportSubjectPublicKeyInfoPem();
    }

    public string JwtPrivateKeyPem { get; }

    public string JwtPublicKeyPem { get; }

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Jwt:Issuer"] = JwtIssuer,
                    ["Jwt:Audience"] = JwtAudience,
                    ["Jwt:AccessTokenMinutes"] = "15",
                    ["Jwt:PrivateKeyPem"] = JwtPrivateKeyPem,
                    ["Jwt:PublicKeyPem"] = JwtPublicKeyPem
                });
        });

        builder.ConfigureServices(services =>
        {
            
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<
                IDbContextOptionsConfiguration<AppDbContext>>();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(connection);
            });

            services
                .AddControllers()
                .AddApplicationPart(
                    typeof(ProtectedTestController).Assembly);
        });
    }

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();
        await ResetDatabaseAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await connection.DisposeAsync();
        await base.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        dbContext.Goals.AddRange(CreateSeedGoals());

        await dbContext.SaveChangesAsync();
    }

    public async Task<int> AddGoalAsync(
    string title = "Integration test goal",
    string category = "Testing",
    GoalPriority priority = GoalPriority.Medium,
    GoalStatus status = GoalStatus.NotStarted,
    DateTime? targetDate = null,
    bool useDefaultTargetDate = true)
    {
        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;

        var goal = new Goal
        {
            Title = title,
            Category = category,
            Description = "Created during an integration test.",
            Priority = priority,
            Status = status,
            TargetDate = useDefaultTargetDate
                ? targetDate ?? now.AddDays(10)
                : targetDate,
            CreatedDate = now,
            UpdatedDate = now
        };

        dbContext.Goals.Add(goal);

        await dbContext.SaveChangesAsync();

        return goal.Id;
    }

    public async Task<Goal?> FindGoalAsync(int id)
    {
        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        return await dbContext.Goals
            .AsNoTracking()
            .FirstOrDefaultAsync(goal => goal.Id == id);
    }

    private static IReadOnlyList<Goal> CreateSeedGoals()
    {
        var now = DateTime.UtcNow;

        return
        [
            new Goal
            {
                Title = "Build Angular client",
                Category = "Frontend",
                Description = "Create the Angular user interface.",
                Priority = GoalPriority.High,
                Status = GoalStatus.NotStarted,
                TargetDate = now.AddDays(20),
                CreatedDate = now.AddDays(-4),
                UpdatedDate = now.AddDays(-4)
            },
            new Goal
            {
                Title = "Complete integration tests",
                Category = "Testing",
                Description = "Test the complete HTTP API pipeline.",
                Priority = GoalPriority.High,
                Status = GoalStatus.InProgress,
                TargetDate = now.AddDays(5),
                CreatedDate = now.AddDays(-3),
                UpdatedDate = now.AddDays(-1)
            },
            new Goal
            {
                Title = "Add API documentation",
                Category = "Documentation",
                Description = "Document all available endpoints.",
                Priority = GoalPriority.Low,
                Status = GoalStatus.NotStarted,
                TargetDate = now.AddDays(12),
                CreatedDate = now.AddDays(-2),
                UpdatedDate = now.AddDays(-2)
            },
            new Goal
            {
                Title = "Implement pagination",
                Category = "Development",
                Description = "Add paginated goal retrieval.",
                Priority = GoalPriority.Medium,
                Status = GoalStatus.Completed,
                TargetDate = now.AddDays(-2),
                CreatedDate = now.AddDays(-10),
                UpdatedDate = now.AddDays(-2)
            },
            new Goal
            {
                Title = "Implement filtering",
                Category = "Development",
                Description = "Filter goals by query parameters.",
                Priority = GoalPriority.High,
                Status = GoalStatus.Completed,
                TargetDate = now.AddDays(-1),
                CreatedDate = now.AddDays(-8),
                UpdatedDate = now.AddDays(-1)
            }
        ];
    }
}