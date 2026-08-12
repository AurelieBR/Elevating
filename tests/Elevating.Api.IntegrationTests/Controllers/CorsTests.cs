using System.Net;

using Elevating.Api.IntegrationTests.Infrastructure;

namespace Elevating.Api.IntegrationTests.Controllers;

public sealed class CorsTests
    : IClassFixture<ElevatingApiFactory>
{
    private readonly HttpClient client;

    public CorsTests(ElevatingApiFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task Preflight_FromConfiguredFrontend_ShouldAllowCredentials()
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Options,
            "/api/goals");

        request.Headers.Add(
            "Origin",
            "http://localhost:4200");
        request.Headers.Add(
            "Access-Control-Request-Method",
            "GET");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(
            "http://localhost:4200",
            response.Headers.GetValues(
                "Access-Control-Allow-Origin").Single());
        Assert.Equal(
            "true",
            response.Headers.GetValues(
                "Access-Control-Allow-Credentials").Single());
    }
}