using Elevating.Application.Interfaces.Repositories;
using Elevating.Domain.Entities;
using Elevating.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Elevating.Infrastructure.Repositories;

public sealed class GoalRepository(AppDbContext dbContext)
    : IGoalRepository
{
    public async Task<(IReadOnlyList<Goal> Items, int TotalCount)>
    GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Goals
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(goal => goal.Status)
            .ThenByDescending(goal => goal.Priority)
            .ThenBy(goal => goal.TargetDate)
            .ThenBy(goal => goal.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
   

    public Task<Goal?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Goals
            .FirstOrDefaultAsync(
                goal => goal.Id == id,
                cancellationToken);
    }

    public Task AddAsync(
        Goal goal,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Goals
            .AddAsync(goal, cancellationToken)
            .AsTask();
    }

    public void Remove(Goal goal)
    {
        dbContext.Goals.Remove(goal);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
