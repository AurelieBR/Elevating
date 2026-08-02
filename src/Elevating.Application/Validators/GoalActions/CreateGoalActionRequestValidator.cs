using Elevating.Application.DTOs.GoalActions;

using FluentValidation;

namespace Elevating.Application.Validators.GoalActions;

public sealed class CreateGoalActionRequestValidator
    : AbstractValidator<CreateGoalActionRequest>
{
    public CreateGoalActionRequestValidator()
    {
        RuleFor(request => request.Title)
            .NotEmpty()
            .MaximumLength(200);
    }
}