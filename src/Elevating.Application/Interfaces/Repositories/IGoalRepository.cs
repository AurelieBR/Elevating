using Elevating.Domain.Entities;
using Elevating.Application.Common.Queries;

namespace Elevating.Application.Interfaces.Repositories;

public interface IGoalRepository
{
    Task<(IReadOnlyList<Goal> Items, int TotalCount)> GetPagedAsync(
        GoalQueryParameters parameters,
        CancellationToken cancellationToken = default);
    Task<GoalSummaryResult> GetSummaryAsync(
    CancellationToken cancellationToken = default);

    Task<Goal?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Goal goal,
        CancellationToken cancellationToken = default);

    void Remove(Goal goal);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
