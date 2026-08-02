using Elevating.Domain.Enums;

namespace Elevating.Domain.Entities;

public sealed class GoalAction
{
    public int Id { get; set; }

    public int GoalId { get; set; }

    public required string Title { get; set; }

    public GoalActionStatus Status { get; set; }
        = GoalActionStatus.Pending;

    public int Position { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public Goal Goal { get; set; } = null!;
}