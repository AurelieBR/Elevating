using Elevating.Domain.Entities;

namespace Elevating.Application.Interfaces.Repositories;

public interface IGoalActionRepository
{
    Task<int> GetNextPositionAsync(
        int goalId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        GoalAction action,
        CancellationToken cancellationToken = default);

    void Remove(GoalAction action);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}