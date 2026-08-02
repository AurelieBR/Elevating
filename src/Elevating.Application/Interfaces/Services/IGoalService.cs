using Elevating.Application.Common.Pagination;
using Elevating.Application.Common.Queries;
using Elevating.Application.Common.Results;
using Elevating.Application.DTOs.Goals;

namespace Elevating.Application.Interfaces.Services;

public interface IGoalService
{
    Task<PagedResult<GoalDto>> GetPagedAsync(
        GoalQueryParameters parameters,
        CancellationToken cancellationToken = default);
    Task<GoalSummaryDto> GetSummaryAsync(
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

    Task<CompleteGoalResult> CompleteAsync(
        int id,
        CompleteGoalRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}