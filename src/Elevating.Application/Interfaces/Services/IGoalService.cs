using Elevating.Application.DTOs.Goals;
using Elevating.Application.Common.Pagination;

namespace Elevating.Application.Interfaces.Services;

public interface IGoalService
{
    Task<PagedResult<GoalDto>> GetPagedAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken = default);

    Task<GoalDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<GoalDto> CreateAsync(
        CreateGoalRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        int id,
        UpdateGoalRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> CompleteAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}
