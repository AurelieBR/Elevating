namespace Elevating.Application.DTOs.Goals;

public sealed record CompleteGoalRequest(
    RemainingActionsResolution? RemainingActionsResolution);