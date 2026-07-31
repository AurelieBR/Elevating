using Elevating.Domain.Enums;

namespace Elevating.Application.DTOs.Goals;

public sealed record GoalDto(
    int Id,
    string Title,
    string Category,
    string? Description,
    GoalPriority Priority,
    GoalStatus Status,
    DateTime? TargetDate,
    DateTime CreatedDate,
    DateTime UpdatedDate,
    bool IsOverdue);