using Elevating.Application.DTOs.GoalActions;
using Elevating.Application.Interfaces.Services;

using FluentValidation;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Elevating.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/goals/{goalId:int}/actions")]
public sealed class GoalActionsController(
    IGoalActionService goalActionService,
    IValidator<CreateGoalActionRequest> createValidator,
    IValidator<UpdateGoalActionRequest> updateValidator)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<GoalActionDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<
        IReadOnlyList<GoalActionDto>>> GetAll(
        int goalId,
        CancellationToken cancellationToken)
    {
        var actions = await goalActionService.GetAllAsync(
            goalId,
            cancellationToken);

        return actions is null
            ? NotFound()
            : Ok(actions);
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(GoalActionDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GoalActionDto>> Create(
        int goalId,
        CreateGoalActionRequest request,
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

        var action = await goalActionService.CreateAsync(
            goalId,
            request,
            cancellationToken);

        if (action is null)
        {
            return NotFound();
        }

        return Created(
            $"/api/goals/{goalId}/actions/{action.Id}",
            action);
    }

    [HttpPut("{actionId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int goalId,
        int actionId,
        UpdateGoalActionRequest request,
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

        var updated = await goalActionService.UpdateAsync(
            goalId,
            actionId,
            request,
            cancellationToken);

        return updated
            ? NoContent()
            : NotFound();
    }

    [HttpPatch("{actionId:int}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(
        int goalId,
        int actionId,
        CancellationToken cancellationToken)
    {
        var completed =
            await goalActionService.CompleteAsync(
                goalId,
                actionId,
                cancellationToken);

        return completed
            ? NoContent()
            : NotFound();
    }

    [HttpPatch("{actionId:int}/reopen")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reopen(
        int goalId,
        int actionId,
        CancellationToken cancellationToken)
    {
        var reopened =
            await goalActionService.ReopenAsync(
                goalId,
                actionId,
                cancellationToken);

        return reopened
            ? NoContent()
            : NotFound();
    }

    [HttpDelete("{actionId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int goalId,
        int actionId,
        CancellationToken cancellationToken)
    {
        var deleted =
            await goalActionService.DeleteAsync(
                goalId,
                actionId,
                cancellationToken);

        return deleted
            ? NoContent()
            : NotFound();
    }
}