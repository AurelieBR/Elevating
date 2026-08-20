using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

using Elevating.Api.Authentication;
using Elevating.Api.ExceptionHandling;
using Elevating.Application.DependencyInjection;
using Elevating.Application.Interfaces.Authentication;
using Elevating.Infrastructure.Authentication;
using Elevating.Infrastructure.DependencyInjection;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
const string FrontendCorsPolicy = "Frontend";

builder.Services.AddCors(options =>
{
    var allowedOrigins =
        builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()
        ?? [];

    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddScoped<
    IRefreshTokenCookieService,
    RefreshTokenCookieService>();

builder.Services
    .AddOptions<RefreshCookieOptions>()
    .Bind(builder.Configuration.GetSection(
        RefreshCookieOptions.SectionName))
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(options.Name) &&
            !string.IsNullOrWhiteSpace(options.Path) &&
            options.Path.StartsWith('/') &&
            Enum.IsDefined(typeof(SameSiteMode), options.SameSite),
        "RefreshCookie configuration is invalid.")
    .ValidateOnStart();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddAuthorization();

builder.Services
    .AddOptions<JwtBearerOptions>(
        JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>(
        (bearerOptions, jwtOptionsAccessor) =>
            ConfigureJwtBearer(
                bearerOptions,
                jwtOptionsAccessor.Value));

var app = builder.Build();

app.UseCors(FrontendCorsPolicy);

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "Elevating API v1");

        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Elevating API";
    });
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet(
    "/api/health",
    () => Results.Ok(
        new
        {
            status = "Healthy",
            application = "Elevating.Api",
            timestampUtc = DateTimeOffset.UtcNow
        }));

app.Run();

static void ConfigureJwtBearer(
    JwtBearerOptions bearerOptions,
    JwtOptions jwtOptions)
{
    using var rsa = RSA.Create();
    rsa.ImportFromPem(jwtOptions.PublicKeyPem);

    bearerOptions.MapInboundClaims = false;
    bearerOptions.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new RsaSecurityKey(
            rsa.ExportParameters(includePrivateParameters: false)),
        RequireSignedTokens = true,
        ValidAlgorithms = [SecurityAlgorithms.RsaSha256],

        ValidateIssuer = true,
        ValidIssuer = jwtOptions.Issuer,

        ValidateAudience = true,
        ValidAudience = jwtOptions.Audience,

        RequireExpirationTime = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,

        NameClaimType = JwtRegisteredClaimNames.Sub
    };
}

public partial class Program;