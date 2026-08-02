using Elevating.Application.Common.Queries;
using Elevating.Application.Interfaces.Repositories;
using Elevating.Domain.Entities;
using Elevating.Domain.Enums;
using Elevating.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Elevating.Infrastructure.Repositories;

public sealed class GoalRepository(AppDbContext dbContext)
    : IGoalRepository
{
    public async Task<(IReadOnlyList<Goal> Items, int TotalCount)>
        GetPagedAsync(
            GoalQueryParameters parameters,
            CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;

        var query = dbContext.Goals
            .AsNoTracking()
            .Include(goal => goal.Actions)
            .AsQueryable();


        if (parameters.Status.HasValue)
        {
            query = query.Where(
                goal => goal.Status == parameters.Status.Value);
        }

        if (parameters.Priority.HasValue)
        {
            query = query.Where(
                goal => goal.Priority == parameters.Priority.Value);
        }

        if (parameters.IsOverdue.HasValue)
        {
            query = parameters.IsOverdue.Value
                ? query.Where(goal =>
                    goal.TargetDate.HasValue &&
                    goal.TargetDate.Value < today &&
                    goal.Status != GoalStatus.Completed)
                : query.Where(goal =>
                    !goal.TargetDate.HasValue ||
                    goal.TargetDate.Value >= today ||
                    goal.Status == GoalStatus.Completed);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Category))
        {
            var category = parameters.Category.Trim();

            query = query.Where(
                goal => goal.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var searchTerm = parameters.Search.Trim();

            query = query.Where(
                goal =>
                    goal.Title.Contains(searchTerm) ||
                    (goal.Description != null &&
                     goal.Description.Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var orderedQuery = ApplySorting(query, parameters);

        var items = await orderedQuery
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<GoalSummaryResult> GetSummaryAsync(
    CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;

        var summary = await dbContext.Goals
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(group => new GoalSummaryResult(
                group.Count(),
                group.Count(goal => goal.Status == GoalStatus.NotStarted),
                group.Count(goal => goal.Status == GoalStatus.InProgress),
                group.Count(goal => goal.Status == GoalStatus.Completed),
                group.Count(goal =>
                    goal.TargetDate.HasValue &&
                    goal.TargetDate.Value < today &&
                    goal.Status != GoalStatus.Completed)))
            .SingleOrDefaultAsync(cancellationToken);

        return summary ?? new GoalSummaryResult(
            Total: 0,
            NotStarted: 0,
            InProgress: 0,
            Completed: 0,
            Overdue: 0);
    }

    public Task<Goal?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Goals
            .Include(goal => goal.Actions)
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

    private static IOrderedQueryable<Goal> ApplySorting(
    IQueryable<Goal> query,
    GoalQueryParameters parameters)
    {
        if (!parameters.SortBy.HasValue)
        {
            return query
                .OrderBy(goal => goal.Status)
                .ThenByDescending(goal => goal.Priority)
                .ThenBy(goal => goal.TargetDate)
                .ThenBy(goal => goal.Id);
        }

        var orderedQuery = parameters.SortDirection
            == SortDirection.Descending
                ? ApplyDescendingSort(query, parameters.SortBy.Value)
                : ApplyAscendingSort(query, parameters.SortBy.Value);

        return orderedQuery.ThenBy(goal => goal.Id);
    }

    private static IOrderedQueryable<Goal> ApplyAscendingSort(
        IQueryable<Goal> query,
        GoalSortBy sortBy)
    {
        return sortBy switch
        {
            GoalSortBy.Title =>
                query.OrderBy(goal => goal.Title),

            GoalSortBy.Category =>
                query.OrderBy(goal => goal.Category),

            GoalSortBy.Priority =>
                query.OrderBy(goal => goal.Priority),

            GoalSortBy.Status =>
                query.OrderBy(goal => goal.Status),

            GoalSortBy.TargetDate =>
                query.OrderBy(goal => goal.TargetDate),

            GoalSortBy.CreatedDate =>
                query.OrderBy(goal => goal.CreatedDate),

            GoalSortBy.UpdatedDate =>
                query.OrderBy(goal => goal.UpdatedDate),

            _ => query.OrderBy(goal => goal.Id)
        };
    }

    private static IOrderedQueryable<Goal> ApplyDescendingSort(
        IQueryable<Goal> query,
        GoalSortBy sortBy)
    {
        return sortBy switch
        {
            GoalSortBy.Title =>
                query.OrderByDescending(goal => goal.Title),

            GoalSortBy.Category =>
                query.OrderByDescending(goal => goal.Category),

            GoalSortBy.Priority =>
                query.OrderByDescending(goal => goal.Priority),

            GoalSortBy.Status =>
                query.OrderByDescending(goal => goal.Status),

            GoalSortBy.TargetDate =>
                query.OrderByDescending(goal => goal.TargetDate),

            GoalSortBy.CreatedDate =>
                query.OrderByDescending(goal => goal.CreatedDate),

            GoalSortBy.UpdatedDate =>
                query.OrderByDescending(goal => goal.UpdatedDate),

            _ => query.OrderByDescending(goal => goal.Id)
        };
    }
}