using Elevating.Api.ExceptionHandling;
using Elevating.Application.DependencyInjection;
using Elevating.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
const string FrontendCorsPolicy = "Frontend";

var allowedOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>()
    ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);



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

public partial class Program;