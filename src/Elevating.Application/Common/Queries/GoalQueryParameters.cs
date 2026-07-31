using System.ComponentModel.DataAnnotations;

using Elevating.Application.Common.Pagination;
using Elevating.Domain.Enums;

namespace Elevating.Application.Common.Queries;

public sealed class GoalQueryParameters : PaginationRequest
{
    public GoalStatus? Status { get; init; }

    public GoalPriority? Priority { get; init; }

    public bool? IsOverdue { get; init; }

    [StringLength(100)]
    public string? Category { get; init; }

    [StringLength(200)]
    public string? Search { get; init; }

    public GoalSortBy? SortBy { get; init; }

    public SortDirection SortDirection { get; init; }
        = SortDirection.Ascending;
}