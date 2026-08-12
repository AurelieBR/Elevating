using Elevating.Application.Common.Pagination;
using Elevating.Application.Common.Queries;
using Elevating.Application.Common.Results;
using Elevating.Application.DTOs.Goals;
using Elevating.Application.Interfaces.Authentication;
using Elevating.Application.Interfaces.Repositories;
using Elevating.Application.Interfaces.Services;
using Elevating.Domain.Entities;
using Elevating.Domain.Enums;

using Microsoft.Extensions.Logging;

namespace Elevating.Application.Services;

public sealed class GoalService(
    IGoalRepository goalRepository,
    ICurrentUser currentUser,
    ILogger<GoalService> logger)
    : IGoalService
{
    public async Task<PagedResult<GoalDto>> GetPagedAsync(
    GoalQueryParameters parameters,
    CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        logger.LogInformation(
            "Retrieving goals page {PageNumber} with page size {PageSize}. " +
            "Filters: Status={Status}, Priority={Priority}, " +
            "IsOverdue={IsOverdue}, Category={Category}, Search={Search}. " +
            "Sorting: SortBy={SortBy}, SortDirection={SortDirection}.",
            parameters.PageNumber,
            parameters.PageSize,
            parameters.Status,
            parameters.Priority,
            parameters.IsOverdue,
            parameters.Category,
            parameters.Search,
            parameters.SortBy,
            parameters.SortDirection);

        var result = await goalRepository.GetPagedAsync(
            GetRequiredOwnerId(),
            parameters,
            cancellationToken);

        var items = result.Items
            .Select(MapToDto)
            .ToList();

        logger.LogInformation(
            "Retrieved {GoalCount} goals from {TotalCount} matching goals.",
            items.Count,
            result.TotalCount);

        return new PagedResult<GoalDto>(
            items,
            parameters.PageNumber,
            parameters.PageSize,
            result.TotalCount);
    }

    public async Task<GoalSummaryDto> GetSummaryAsync(
    CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Retrieving goal dashboard summary.");

        var summary = await goalRepository.GetSummaryAsync(
            GetRequiredOwnerId(),
            cancellationToken);

        logger.LogInformation(
            "Retrieved goal summary. Total={Total}, " +
            "NotStarted={NotStarted}, InProgress={InProgress}, " +
            "Completed={Completed}, Overdue={Overdue}.",
            summary.Total,
            summary.NotStarted,
            summary.InProgress,
            summary.Completed,
            summary.Overdue);

        return new GoalSummaryDto(
            summary.Total,
            summary.NotStarted,
            summary.InProgress,
            summary.Completed,
            summary.Overdue);
    }

    public async Task<GoalDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var goal = await goalRepository.GetByIdAsync(
            GetRequiredOwnerId(),
            id,
            cancellationToken);

        return goal is null
            ? null
            : MapToDto(goal);
    }

    public async Task<GoalDto> CreateAsync(
        CreateGoalRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        logger.LogInformation(
    "Creating goal '{Title}' in category '{Category}'.",
    request.Title,
    request.Category);

        var now = DateTime.UtcNow;

        var goal = new Goal
        {
            OwnerId = GetRequiredOwnerId(),
            Title = request.Title.Trim(),
            Category = request.Category.Trim(),
            Description = NormalizeOptionalText(request.Description),
            Priority = request.Priority,
            Status = GoalStatus.NotStarted,
            TargetDate = request.TargetDate,
            CreatedDate = now,
            UpdatedDate = now
        };

        await goalRepository.AddAsync(goal, cancellationToken);
        await goalRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
    "Goal {GoalId} created successfully.",
    goal.Id);

        return MapToDto(goal);
    }

    private static GoalStatus CalculateActionDrivenStatus(
    Goal goal)
    {
        if (goal.Actions.Count == 0)
        {
            return goal.Status;
        }

        var pendingCount = goal.Actions.Count(
            action =>
                action.Status == GoalActionStatus.Pending);

        if (pendingCount == 0)
        {
            return GoalStatus.Completed;
        }

        var completedCount = goal.Actions.Count(
            action =>
                action.Status == GoalActionStatus.Completed);

        return completedCount > 0
            ? GoalStatus.InProgress
            : GoalStatus.NotStarted;
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateGoalRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        logger.LogInformation(
    "Updating goal {GoalId}.",
    id);

        var goal = await goalRepository.GetByIdAsync(
            GetRequiredOwnerId(),
            id,
            cancellationToken);

        if (goal is null)
        {
            logger.LogWarning(
       "Goal {GoalId} was not found.",
       id);
            return false;
        }

        goal.Title = request.Title.Trim();
        goal.Category = request.Category.Trim();
        goal.Description = NormalizeOptionalText(request.Description);
        goal.Priority = request.Priority;
        goal.Status = goal.Actions.Count == 0 ? request.Status : CalculateActionDrivenStatus(goal);
        goal.TargetDate = request.TargetDate;
        goal.UpdatedDate = DateTime.UtcNow;

        await goalRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
    "Goal {GoalId} updated successfully.",
    id);

        return true;
    }

    public async Task<CompleteGoalResult> CompleteAsync(
        int id,
        CompleteGoalRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        logger.LogInformation(
            "Completing goal {GoalId}.",
            id);

        var goal = await goalRepository.GetByIdAsync(
            GetRequiredOwnerId(),
            id,
            cancellationToken);

        if (goal is null)
        {
            logger.LogWarning(
                "Goal {GoalId} was not found.",
                id);

            return CompleteGoalResult.NotFound;
        }

        var resolution =
            request.RemainingActionsResolution;

        if (resolution.HasValue &&
            !Enum.IsDefined(
                typeof(RemainingActionsResolution),
                resolution.Value))
        {
            return CompleteGoalResult.InvalidResolution;
        }

        var pendingActions = goal.Actions
            .Where(action =>
                action.Status == GoalActionStatus.Pending)
            .ToList();

        if (pendingActions.Count > 0 &&
            !resolution.HasValue)
        {
            return CompleteGoalResult.ResolutionRequired;
        }

        var now = DateTime.UtcNow;

        if (pendingActions.Count > 0)
        {
            var newStatus =
                resolution == RemainingActionsResolution.Complete
                    ? GoalActionStatus.Completed
                    : GoalActionStatus.Skipped;

            foreach (var action in pendingActions)
            {
                action.Status = newStatus;
                action.UpdatedDate = now;
            }
        }

        goal.Status = GoalStatus.Completed;
        goal.UpdatedDate = now;

        await goalRepository.SaveChangesAsync(
            cancellationToken);

        logger.LogInformation(
            "Goal {GoalId} completed successfully.",
            id);

        return CompleteGoalResult.Completed;
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
        "Deleting goal {GoalId}.",
        id);

        var goal = await goalRepository.GetByIdAsync(
            GetRequiredOwnerId(),
            id,
            cancellationToken);

        if (goal is null)
        {
            logger.LogWarning(
            "Goal {GoalId} was not found.",
            id);

            return false;
        }

        goalRepository.Remove(goal);
        await goalRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
       "Goal {GoalId} deleted successfully.",
       id);

        return true;
    }

    private static GoalDto MapToDto(Goal goal)
    {
        var isOverdue =
            goal.TargetDate.HasValue &&
            goal.TargetDate.Value < DateTime.UtcNow.Date &&
            goal.Status != GoalStatus.Completed;

        var actionCount = goal.Actions.Count;

        var completedActionCount = goal.Actions.Count(
            action =>
                action.Status == GoalActionStatus.Completed);

        var skippedActionCount = goal.Actions.Count(
            action =>
                action.Status == GoalActionStatus.Skipped);

        var pendingActionCount = goal.Actions.Count(
            action =>
                action.Status == GoalActionStatus.Pending);

        var requiredActionCount =
            actionCount - skippedActionCount;

        var progressPercentage =
            goal.Status == GoalStatus.Completed
                ? 100
                : requiredActionCount == 0
                    ? 0
                    : (int)Math.Round(
                        completedActionCount * 100.0 /
                        requiredActionCount);

        return new GoalDto(
            goal.Id,
            goal.Title,
            goal.Category,
            goal.Description,
            goal.Priority,
            goal.Status,
            goal.TargetDate,
            goal.CreatedDate,
            goal.UpdatedDate,
            isOverdue,
            actionCount,
            completedActionCount,
            skippedActionCount,
            pendingActionCount,
            progressPercentage);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
    private Guid GetRequiredOwnerId()
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            throw new InvalidOperationException(
                "An authenticated current user is required for goal operations.");
        }

        return currentUser.UserId.Value;
    }
}