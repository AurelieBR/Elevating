using System;
using System.Collections.Generic;
using System.Text;

namespace Elevating.Application.Common.Queries;

public sealed record GoalSummaryResult(
    int Total,
    int NotStarted,
    int InProgress,
    int Completed,
    int Overdue);
