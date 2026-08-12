using Elevating.Application.Common.Queries;
using Elevating.Domain.Entities;

namespace Elevating.Application.Interfaces.Repositories;

public interface IGoalRepository
{
    Task<(IReadOnlyList<Goal> Items, int TotalCount)> GetPagedAsync(
        Guid ownerId,
        GoalQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<GoalSummaryResult> GetSummaryAsync(
        Guid ownerId,
        CancellationToken cancellationToken = default);

    Task<Goal?> GetByIdAsync(
        Guid ownerId,
        int id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Goal goal,
        CancellationToken cancellationToken = default);

    void Remove(Goal goal);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}