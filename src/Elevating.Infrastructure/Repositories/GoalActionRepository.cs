using Elevating.Application.Interfaces.Repositories;
using Elevating.Domain.Entities;
using Elevating.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Elevating.Infrastructure.Repositories;

public sealed class GoalActionRepository(
    AppDbContext dbContext)
    : IGoalActionRepository
{
    public Task<GoalAction?> GetByIdAsync(
        Guid ownerId,
        int goalId,
        int actionId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.GoalActions
            .Include(action => action.Goal)
            .ThenInclude(goal => goal.Actions)
            .FirstOrDefaultAsync(
                action =>
                    action.Id == actionId &&
                    action.GoalId == goalId &&
                    action.Goal.OwnerId == ownerId,
                cancellationToken);
    }

    public async Task<int> GetNextPositionAsync(
        Guid ownerId,
        int goalId,
        CancellationToken cancellationToken = default)
    {
        var highestPosition = await dbContext.GoalActions
            .Where(action =>
                action.GoalId == goalId &&
                action.Goal.OwnerId == ownerId)
            .Select(action => (int?)action.Position)
            .MaxAsync(cancellationToken);

        return (highestPosition ?? 0) + 1;
    }

    public Task AddAsync(
        GoalAction action,
        CancellationToken cancellationToken = default)
    {
        return dbContext.GoalActions
            .AddAsync(action, cancellationToken)
            .AsTask();
    }

    public void Remove(GoalAction action)
    {
        dbContext.GoalActions.Remove(action);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}