using Elevating.Application.DTOs.GoalActions;
using Elevating.Application.Interfaces.Repositories;
using Elevating.Application.Interfaces.Services;
using Elevating.Domain.Entities;
using Elevating.Domain.Enums;

using Microsoft.Extensions.Logging;

namespace Elevating.Application.Services;

public sealed class GoalActionService(
    IGoalRepository goalRepository,
    IGoalActionRepository goalActionRepository,
    ILogger<GoalActionService> logger)
    : IGoalActionService
{
    public async Task<IReadOnlyList<GoalActionDto>?> GetAllAsync(
        int goalId,
        CancellationToken cancellationToken = default)
    {
        var goal = await goalRepository.GetByIdAsync(
            goalId,
            cancellationToken);

        if (goal is null)
        {
            return null;
        }

        return goal.Actions
            .OrderBy(action => action.Position)
            .Select(MapToDto)
            .ToList();
    }

    public async Task<GoalActionDto?> CreateAsync(
        int goalId,
        CreateGoalActionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var goal = await goalRepository.GetByIdAsync(
            goalId,
            cancellationToken);

        if (goal is null)
        {
            return null;
        }

        var now = DateTime.UtcNow;

        var action = new GoalAction
        {
            GoalId = goalId,
            Title = request.Title.Trim(),
            Status = GoalActionStatus.Pending,
            Position =
                await goalActionRepository.GetNextPositionAsync(
                    goalId,
                    cancellationToken),
            CreatedDate = now,
            UpdatedDate = now
        };

        await goalActionRepository.AddAsync(
            action,
            cancellationToken);

        goal.Actions.Add(action);

        if (goal.Status == GoalStatus.Completed)
        {
            goal.Status = GoalStatus.InProgress;
        }

        goal.UpdatedDate = now;

        await goalActionRepository.SaveChangesAsync(
            cancellationToken);

        logger.LogInformation(
            "Action {ActionId} created for goal {GoalId}.",
            action.Id,
            goalId);

        return MapToDto(action);
    }

    public async Task<bool> UpdateAsync(
        int goalId,
        int actionId,
        UpdateGoalActionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var goal = await goalRepository.GetByIdAsync(
            goalId,
            cancellationToken);

        var action = goal?.Actions.FirstOrDefault(
            candidate => candidate.Id == actionId);

        if (goal is null || action is null)
        {
            return false;
        }

        var now = DateTime.UtcNow;

        action.Title = request.Title.Trim();
        action.UpdatedDate = now;
        goal.UpdatedDate = now;

        await goalActionRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> CompleteAsync(
        int goalId,
        int actionId,
        CancellationToken cancellationToken = default)
    {
        var goal = await goalRepository.GetByIdAsync(
            goalId,
            cancellationToken);

        var action = goal?.Actions.FirstOrDefault(
            candidate => candidate.Id == actionId);

        if (goal is null || action is null)
        {
            return false;
        }

        var now = DateTime.UtcNow;

        action.Status = GoalActionStatus.Completed;
        action.UpdatedDate = now;

        SynchronizeGoalStatus(goal, now);

        await goalActionRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> ReopenAsync(
        int goalId,
        int actionId,
        CancellationToken cancellationToken = default)
    {
        var goal = await goalRepository.GetByIdAsync(
            goalId,
            cancellationToken);

        var action = goal?.Actions.FirstOrDefault(
            candidate => candidate.Id == actionId);

        if (goal is null || action is null)
        {
            return false;
        }

        var now = DateTime.UtcNow;

        action.Status = GoalActionStatus.Pending;
        action.UpdatedDate = now;

        SynchronizeGoalStatus(goal, now);

        await goalActionRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(
        int goalId,
        int actionId,
        CancellationToken cancellationToken = default)
    {
        var goal = await goalRepository.GetByIdAsync(
            goalId,
            cancellationToken);

        var action = goal?.Actions.FirstOrDefault(
            candidate => candidate.Id == actionId);

        if (goal is null || action is null)
        {
            return false;
        }

        var now = DateTime.UtcNow;

        goal.Actions.Remove(action);
        goalActionRepository.Remove(action);

        if (goal.Actions.Count > 0)
        {
            SynchronizeGoalStatus(goal, now);
        }
        else
        {
            goal.UpdatedDate = now;
        }

        await goalActionRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    private static void SynchronizeGoalStatus(
        Goal goal,
        DateTime now)
    {
        if (goal.Actions.Count == 0)
        {
            goal.UpdatedDate = now;
            return;
        }

        var pendingCount = goal.Actions.Count(
            action =>
                action.Status == GoalActionStatus.Pending);

        var completedCount = goal.Actions.Count(
            action =>
                action.Status == GoalActionStatus.Completed);

        goal.Status = pendingCount == 0
            ? GoalStatus.Completed
            : completedCount > 0
                ? GoalStatus.InProgress
                : GoalStatus.NotStarted;

        goal.UpdatedDate = now;
    }

    private static GoalActionDto MapToDto(
        GoalAction action)
    {
        return new GoalActionDto(
            action.Id,
            action.GoalId,
            action.Title,
            action.Status,
            action.Position,
            action.CreatedDate,
            action.UpdatedDate);
    }
}