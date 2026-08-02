using Elevating.Domain.Enums;

namespace Elevating.Application.DTOs.GoalActions;

public sealed record GoalActionDto(
    int Id,
    int GoalId,
    string Title,
    GoalActionStatus Status,
    int Position,
    DateTime CreatedDate,
    DateTime UpdatedDate);