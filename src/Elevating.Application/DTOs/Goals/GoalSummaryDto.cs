using System;
using System.Collections.Generic;
using System.Text;

namespace Elevating.Application.DTOs.Goals;

public sealed record GoalSummaryDto(
    int Total,
    int NotStarted,
    int InProgress,
    int Completed,
    int Overdue);