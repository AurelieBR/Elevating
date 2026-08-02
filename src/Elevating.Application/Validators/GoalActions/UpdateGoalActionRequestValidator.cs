using Elevating.Application.DTOs.GoalActions;

using FluentValidation;

namespace Elevating.Application.Validators.GoalActions;

public sealed class UpdateGoalActionRequestValidator
    : AbstractValidator<UpdateGoalActionRequest>
{
    public UpdateGoalActionRequestValidator()
    {
        RuleFor(request => request.Title)
            .NotEmpty()
            .MaximumLength(200);
    }
}