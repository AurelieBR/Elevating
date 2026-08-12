using System.Security.Cryptography;

using Microsoft.Extensions.Options;

namespace Elevating.Infrastructure.Authentication;

internal sealed class JwtOptionsValidator
    : IValidateOptions<JwtOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        JwtOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            failures.Add("Jwt:Issuer is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            failures.Add("Jwt:Audience is required.");
        }

        if (options.AccessTokenMinutes is < 1 or > 1440)
        {
            failures.Add(
                "Jwt:AccessTokenMinutes must be between 1 and 1440.");
        }

        ValidateSigningKeys(options, failures);

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidateSigningKeys(
        JwtOptions options,
        ICollection<string> failures)
    {
        if (string.IsNullOrWhiteSpace(options.PrivateKeyPem))
        {
            failures.Add("Jwt:PrivateKeyPem is required.");
        }

        if (string.IsNullOrWhiteSpace(options.PublicKeyPem))
        {
            failures.Add("Jwt:PublicKeyPem is required.");
        }

        if (failures.Count > 0)
        {
            return;
        }

        try
        {
            using var privateRsa = RSA.Create();
            privateRsa.ImportFromPem(options.PrivateKeyPem);

            using var publicRsa = RSA.Create();
            publicRsa.ImportFromPem(options.PublicKeyPem);

            if (privateRsa.KeySize < 2048 || publicRsa.KeySize < 2048)
            {
                failures.Add(
                    "Jwt signing keys must use RSA with at least 2048 bits.");
            }

            var privateParameters =
                privateRsa.ExportParameters(includePrivateParameters: true);

            var publicParameters =
                publicRsa.ExportParameters(includePrivateParameters: false);

            if (privateParameters.Modulus is null ||
                publicParameters.Modulus is null ||
                privateParameters.Exponent is null ||
                publicParameters.Exponent is null ||
                !privateParameters.Modulus.SequenceEqual(
                    publicParameters.Modulus) ||
                !privateParameters.Exponent.SequenceEqual(
                    publicParameters.Exponent))
            {
                failures.Add(
                    "Jwt signing private and public keys do not match.");
            }
        }
        catch (CryptographicException)
        {
            failures.Add(
                "Jwt signing keys must contain valid RSA PEM data.");
        }
        catch (ArgumentException)
        {
            failures.Add(
                "Jwt signing keys must contain valid RSA PEM data.");
        }
    }
}