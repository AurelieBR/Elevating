using Elevating.Domain.Entities;

namespace Elevating.Application.Interfaces.Repositories;

public interface IGoalActionRepository
{
    Task<GoalAction?> GetByIdAsync(
        Guid ownerId,
        int goalId,
        int actionId,
        CancellationToken cancellationToken = default);

    Task<int> GetNextPositionAsync(
        Guid ownerId,
        int goalId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        GoalAction action,
        CancellationToken cancellationToken = default);

    void Remove(GoalAction action);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}