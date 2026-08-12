using Elevating.Api.Authentication;
using Elevating.Application.Common.Authentication;
using Elevating.Application.DTOs.Authentication;
using Elevating.Application.Interfaces.Authentication;
using Elevating.Application.Interfaces.Services;

using FluentValidation;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Elevating.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IAuthenticationService authenticationService,
    IRefreshTokenCookieService refreshTokenCookieService,
    ICurrentUser currentUser,
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
                CompleteAuthentication(result),

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
            ? CompleteAuthentication(result)
            : LoginFailed();
    }

    [HttpPost("refresh")]
    [ProducesResponseType(
        typeof(AuthenticationResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResponse>> Refresh(
        CancellationToken cancellationToken)
    {
        var refreshToken = refreshTokenCookieService.Read(Request);

        var result = await authenticationService.RefreshAsync(
            refreshToken ?? string.Empty,
            cancellationToken);

        if (result.Status != AuthenticationStatus.Succeeded)
        {
            refreshTokenCookieService.Clear(Response);
            return SessionFailed();
        }

        return CompleteAuthentication(result);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        CancellationToken cancellationToken)
    {
        var refreshToken = refreshTokenCookieService.Read(Request);

        await authenticationService.LogoutAsync(
            refreshToken,
            cancellationToken);

        refreshTokenCookieService.Clear(Response);

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(
        typeof(CurrentUserResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentUserResponse>> Me(
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated ||
            !currentUser.UserId.HasValue)
        {
            return Unauthorized();
        }

        var user = await authenticationService.GetCurrentUserAsync(
            currentUser.UserId.Value,
            cancellationToken);

        return user is null
            ? Unauthorized()
            : Ok(user);
    }

    private ActionResult<AuthenticationResponse>
        CompleteAuthentication(AuthenticationResult result)
    {
        if (result.Response is null || result.RefreshToken is null)
        {
            throw new InvalidOperationException(
                "A successful authentication result requires " +
                "access and refresh tokens.");
        }

        refreshTokenCookieService.Write(
            Response,
            result.RefreshToken);

        return Ok(result.Response);
    }

    private UnauthorizedObjectResult LoginFailed()
    {
        return Unauthorized(new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Authentication failed",
            Detail = "Invalid email or password."
        });
    }

    private UnauthorizedObjectResult SessionFailed()
    {
        return Unauthorized(new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Authentication failed",
            Detail = "Invalid authentication session."
        });
    }
}