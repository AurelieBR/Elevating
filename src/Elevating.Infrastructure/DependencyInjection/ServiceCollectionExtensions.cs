using Elevating.Application.Interfaces.Authentication;
using Elevating.Application.Interfaces.Repositories;
using Elevating.Infrastructure.Authentication;
using Elevating.Infrastructure.Identity;
using Elevating.Infrastructure.Persistence;
using Elevating.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Elevating.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 10;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan =
                    TimeSpan.FromMinutes(5);
            })
            .AddEntityFrameworkStores<AppDbContext>();

        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<
            IValidateOptions<JwtOptions>,
            JwtOptionsValidator>();

        services
            .AddOptions<RefreshTokenOptions>()
            .Bind(configuration.GetSection(
                RefreshTokenOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<
            IValidateOptions<RefreshTokenOptions>,
            RefreshTokenOptionsValidator>();

        services.TryAddSingleton<TimeProvider>(TimeProvider.System);

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IAccessTokenService, JwtAccessTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        services.AddScoped<IGoalRepository, GoalRepository>();

        services.AddScoped<IGoalActionRepository, GoalActionRepository>();

        return services;
    }
}