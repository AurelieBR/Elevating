using Elevating.Domain.Entities;

namespace Elevating.Application.Interfaces.Repositories;

public interface IGoalRepository
{
    Task<IReadOnlyList<Goal>> GetAllAsync(
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
