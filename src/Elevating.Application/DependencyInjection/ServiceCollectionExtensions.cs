using Elevating.Application.Interfaces.Services;
using Elevating.Application.Services;
using Elevating.Application.Validators.Goals;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

namespace Elevating.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IGoalService, GoalService>();

        services.AddScoped<IGoalActionService, GoalActionService>();

        services.AddValidatorsFromAssemblyContaining<
            CreateGoalRequestValidator>();

        return services;
    }
}