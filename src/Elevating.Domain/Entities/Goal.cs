using Elevating.Domain.Enums;

namespace Elevating.Domain.Entities;

public sealed class Goal
{
    public int Id { get; set; }

    public Guid? OwnerId { get; set; }

    public required string Title { get; set; }

    public required string Category { get; set; }

    public string? Description { get; set; }

    public GoalPriority Priority { get; set; } = GoalPriority.Medium;

    public GoalStatus Status { get; set; } = GoalStatus.NotStarted;

    public DateTime? TargetDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public ICollection<GoalAction> Actions { get; set; }
    = new List<GoalAction>();
}
