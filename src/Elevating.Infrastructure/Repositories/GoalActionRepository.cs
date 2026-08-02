using Elevating.Application.Interfaces.Repositories;
using Elevating.Domain.Entities;
using Elevating.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Elevating.Infrastructure.Repositories;

public sealed class GoalActionRepository(
    AppDbContext dbContext)
    : IGoalActionRepository
{
    public async Task<int> GetNextPositionAsync(
        int goalId,
        CancellationToken cancellationToken = default)
    {
        var highestPosition = await dbContext.GoalActions
            .Where(action => action.GoalId == goalId)
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