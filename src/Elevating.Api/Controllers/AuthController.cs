using Elevating.Application.Common.Authentication;
using Elevating.Application.DTOs.Authentication;
using Elevating.Application.Interfaces.Services;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;

namespace Elevating.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IAuthenticationService authenticationService,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator)
    : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(
        typeof(AuthenticationResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthenticationResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await registerValidator.ValidateAsync(
            request,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            return ValidationProblem(
                new ValidationProblemDetails(
                    validationResult.ToDictionary()));
        }

        var result = await authenticationService.RegisterAsync(
            request,
            cancellationToken);

        return result.Status switch
        {
            AuthenticationStatus.Succeeded =>
                Ok(result.Response),

            AuthenticationStatus.DuplicateEmail =>
                Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Registration failed",
                    Detail =
                        "An account with this email already exists."
                }),

            _ => BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Registration failed",
                Detail = "Registration could not be completed."
            })
        };
    }

    [HttpPost("login")]
    [ProducesResponseType(
        typeof(AuthenticationResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await loginValidator.ValidateAsync(
            request,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            return ValidationProblem(
                new ValidationProblemDetails(
                    validationResult.ToDictionary()));
        }

        var result = await authenticationService.LoginAsync(
            request,
            cancellationToken);

        return result.Status == AuthenticationStatus.Succeeded
            ? Ok(result.Response)
            : Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Authentication failed",
                Detail = "Invalid email or password."
            });
    }
}