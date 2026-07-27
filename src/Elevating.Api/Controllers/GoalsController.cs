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
        typeof(IReadOnlyList<GoalDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<GoalDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var goals = await goalService.GetAllAsync(cancellationToken);

        return Ok(goals);
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
