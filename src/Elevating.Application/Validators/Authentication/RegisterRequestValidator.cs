using Elevating.Application.DTOs.Authentication;

using FluentValidation;

namespace Elevating.Application.Validators.Authentication;

public sealed class RegisterRequestValidator
    : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(request => request.Password)
            .NotEmpty()
            .MinimumLength(10)
            .Matches("[A-Z]")
            .WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]")
            .WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]")
            .WithMessage("Password must contain a digit.");
    }
}