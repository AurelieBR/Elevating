using Elevating.Domain.Enums;

namespace Elevating.Application.DTOs.Goals;

public sealed record CreateGoalRequest(
    string Title,
    string Category,
    string? Description,
    GoalPriority Priority,
    DateTime? TargetDate);
