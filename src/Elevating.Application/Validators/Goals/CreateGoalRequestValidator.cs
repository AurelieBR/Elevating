using Elevating.Application.DTOs.Goals;
using Elevating.Domain.Enums;
using FluentValidation;

namespace Elevating.Application.Validators.Goals;

public sealed class CreateGoalRequestValidator
    : AbstractValidator<CreateGoalRequest>
{
    public CreateGoalRequestValidator()
    {
        RuleFor(request => request.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Category)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.Description)
            .MaximumLength(2000);

        RuleFor(request => request.Priority)
            .IsInEnum();

        RuleFor(request => request.TargetDate)
            .Must(targetDate =>
                !targetDate.HasValue ||
                targetDate.Value.Date >= DateTime.UtcNow.Date)
            .WithMessage(
                "Target date cannot be earlier than today.");
    }
}
