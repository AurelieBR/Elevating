using Elevating.Application.DTOs.GoalActions;

namespace Elevating.Application.Interfaces.Services;

public interface IGoalActionService
{
    Task<IReadOnlyList<GoalActionDto>?> GetAllAsync(
        int goalId,
        CancellationToken cancellationToken = default);

    Task<GoalActionDto?> CreateAsync(
        int goalId,
        CreateGoalActionRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        int goalId,
        int actionId,
        UpdateGoalActionRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> CompleteAsync(
        int goalId,
        int actionId,
        CancellationToken cancellationToken = default);

    Task<bool> ReopenAsync(
        int goalId,
        int actionId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int goalId,
        int actionId,
        CancellationToken cancellationToken = default);
}