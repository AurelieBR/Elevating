using Elevating.Domain.Enums;

namespace Elevating.Application.DTOs.Goals;

public sealed record UpdateGoalRequest(
    string Title,
    string Category,
    string? Description,
    GoalPriority Priority,
    GoalStatus Status,
    DateTime? TargetDate);