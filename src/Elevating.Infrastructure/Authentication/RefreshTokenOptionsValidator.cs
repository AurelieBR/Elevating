using Microsoft.Extensions.Options;

namespace Elevating.Infrastructure.Authentication;

internal sealed class RefreshTokenOptionsValidator
    : IValidateOptions<RefreshTokenOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        RefreshTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.LifetimeDays is >= 1 and <= 90
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(
                "RefreshToken:LifetimeDays must be between 1 and 90.");
    }
}