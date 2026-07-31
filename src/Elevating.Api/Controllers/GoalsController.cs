using Elevating.Application.Common.Pagination;
using Elevating.Application.Common.Queries;
using Elevating.Application.DTOs.Goals;
using Elevating.Application.Interfaces.Services;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;

namespace Elevating.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class GoalsController(
    IGoalService goalService,
    IValidator<CreateGoalRequest> createValidator,
    IValidator<UpdateGoalRequest> updateValidator)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResult<GoalDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<GoalDto>>> GetAll(
    [FromQuery] GoalQueryParameters parameters,
    CancellationToken cancellationToken)
    {
        var result = await goalService.GetPagedAsync(
            parameters,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("summary")]
    [ProducesResponseType(
    typeof(GoalSummaryDto),
    StatusCodes.Status200OK)]
    public async Task<ActionResult<GoalSummaryDto>> GetSummary(
    CancellationToken cancellationToken)
    {
        var summary = await goalService.GetSummaryAsync(
            cancellationToken);

        return Ok(summary);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(GoalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GoalDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var goal = await goalService.GetByIdAsync(
            id,
            cancellationToken);

        return goal is null
            ? NotFound()
            : Ok(goal);
    }

    [HttpPost]
    [ProducesResponseType(typeof(GoalDto), StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GoalDto>> Create(
        CreateGoalRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult =
            await createValidator.ValidateAsync(
                request,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            return ValidationProblem(
				new ValidationProblemDetails(
					validationResult.ToDictionary()));
        }

        var goal = await goalService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = goal.Id },
            goal);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        UpdateGoalRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult =
            await updateValidator.ValidateAsync(
                request,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            return ValidationProblem(
				new ValidationProblemDetails(
					validationResult.ToDictionary()));
        }

        var updated = await goalService.UpdateAsync(
            id,
            request,
            cancellationToken);

        return updated
            ? NoContent()
            : NotFound();
    }

    [HttpPatch("{id:int}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(
        int id,
        CancellationToken cancellationToken)
    {
        var completed = await goalService.CompleteAsync(
            id,
            cancellationToken);

        return completed
            ? NoContent()
            : NotFound();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await goalService.DeleteAsync(
            id,
            cancellationToken);

        return deleted
            ? NoContent()
            : NotFound();
    }
}
