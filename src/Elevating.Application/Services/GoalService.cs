using Elevating.Application.DTOs.Goals;
using Elevating.Application.Interfaces.Repositories;
using Elevating.Application.Interfaces.Services;
using Elevating.Domain.Entities;
using Elevating.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Elevating.Application.Services;

public sealed class GoalService(
    IGoalRepository goalRepository,
    ILogger<GoalService> logger)
    : IGoalService
{
    public async Task<IReadOnlyList<GoalDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var goals = await goalRepository.GetAllAsync(cancellationToken);

        return goals
            .Select(MapToDto)
            .ToList();
    }

    public async Task<GoalDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var goal = await goalRepository.GetByIdAsync(id, cancellationToken);

        return goal is null
            ? null
            : MapToDto(goal);
    }

    public async Task<GoalDto> CreateAsync(
        CreateGoalRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var now = DateTime.UtcNow;

        var goal = new Goal
        {
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

        return MapToDto(goal);
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateGoalRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var goal = await goalRepository.GetByIdAsync(id, cancellationToken);

        if (goal is null)
        {
            return false;
        }

        goal.Title = request.Title.Trim();
        goal.Category = request.Category.Trim();
        goal.Description = NormalizeOptionalText(request.Description);
        goal.Priority = request.Priority;
        goal.Status = request.Status;
        goal.TargetDate = request.TargetDate;
        goal.UpdatedDate = DateTime.UtcNow;

        await goalRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CompleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var goal = await goalRepository.GetByIdAsync(id, cancellationToken);

        if (goal is null)
        {
            return false;
        }

        goal.Status = GoalStatus.Completed;
        goal.UpdatedDate = DateTime.UtcNow;

        await goalRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var goal = await goalRepository.GetByIdAsync(id, cancellationToken);

        if (goal is null)
        {
            return false;
        }

        goalRepository.Remove(goal);
        await goalRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static GoalDto MapToDto(Goal goal)
    {
        return new GoalDto(
            goal.Id,
            goal.Title,
            goal.Category,
            goal.Description,
            goal.Priority,
            goal.Status,
            goal.TargetDate,
            goal.CreatedDate,
            goal.UpdatedDate);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
