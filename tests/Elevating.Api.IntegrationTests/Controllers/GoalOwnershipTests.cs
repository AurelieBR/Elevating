using System.Net;
using System.Net.Http.Json;

using Elevating.Api.IntegrationTests.Infrastructure;
using Elevating.Application.Common.Pagination;
using Elevating.Application.DTOs.Authentication;
using Elevating.Application.DTOs.GoalActions;
using Elevating.Application.DTOs.Goals;
using Elevating.Domain.Enums;

using Microsoft.AspNetCore.Mvc.Testing;

namespace Elevating.Api.IntegrationTests.Controllers;

public sealed class GoalOwnershipTests
    : IClassFixture<ElevatingApiFactory>
{
    private readonly ElevatingApiFactory factory;

    public GoalOwnershipTests(ElevatingApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Create_ShouldAssignAuthenticatedOwnerAndIgnoreSpoofedOwnerId()
    {
        var users = await CreateUsersAsync();

        var aliceResponse = await users.AliceClient.PostAsJsonAsync(
            "/api/goals",
            new
            {
                OwnerId = users.Bob.UserId,
                Title = "Alice goal",
                Category = "Ownership",
                Description = "Owned by the access-token subject.",
                Priority = GoalPriority.High,
                TargetDate = (DateTime?)null
            });

        var bobGoal = await CreateGoalAsync(
            users.BobClient,
            "Bob goal");

        Assert.Equal(HttpStatusCode.Created, aliceResponse.StatusCode);

        var aliceGoal = await aliceResponse.Content
            .ReadFromJsonAsync<GoalDto>();

        Assert.NotNull(aliceGoal);

        var storedAliceGoal = await factory.FindGoalAsync(aliceGoal.Id);
        var storedBobGoal = await factory.FindGoalAsync(bobGoal.Id);

        Assert.NotNull(storedAliceGoal);
        Assert.NotNull(storedBobGoal);
        Assert.Equal(users.Alice.UserId, storedAliceGoal.OwnerId);
        Assert.Equal(users.Bob.UserId, storedBobGoal.OwnerId);
    }

    [Fact]
    public async Task ListsSearchFiltersPaginationAndSummary_ShouldBeOwnerScoped()
    {
        var users = await CreateUsersAsync();

        await CreateGoalAsync(
            users.AliceClient,
            "Alice searchable secret",
            GoalPriority.High);
        await CreateGoalAsync(
            users.AliceClient,
            "Alice second goal",
            GoalPriority.Low);
        await CreateGoalAsync(
            users.BobClient,
            "Bob searchable secret",
            GoalPriority.High);

        var aliceList = await users.AliceClient.GetFromJsonAsync<
            PagedResult<GoalDto>>(
            "/api/goals?pageSize=1&sortBy=Title&sortDirection=Ascending");
        var bobList = await users.BobClient.GetFromJsonAsync<
            PagedResult<GoalDto>>(
            "/api/goals?pageSize=20");
        var aliceSearch = await users.AliceClient.GetFromJsonAsync<
            PagedResult<GoalDto>>(
            "/api/goals?search=searchable&priority=High&pageSize=20");
        var aliceSummary = await users.AliceClient.GetFromJsonAsync<
            GoalSummaryDto>("/api/goals/summary");

        Assert.NotNull(aliceList);
        Assert.Equal(2, aliceList.TotalCount);
        Assert.Single(aliceList.Items);
        Assert.NotNull(bobList);
        Assert.Equal(1, bobList.TotalCount);
        Assert.All(bobList.Items, goal =>
            Assert.StartsWith("Bob", goal.Title));
        Assert.NotNull(aliceSearch);
        Assert.Single(aliceSearch.Items);
        Assert.Equal("Alice searchable secret", aliceSearch.Items[0].Title);
        Assert.NotNull(aliceSummary);
        Assert.Equal(2, aliceSummary.Total);
    }

    [Fact]
    public async Task GetUpdateCompleteAndDelete_ShouldReturnNotFoundForOtherOwner()
    {
        var users = await CreateUsersAsync();
        var goal = await CreateGoalAsync(
            users.AliceClient,
            "Alice protected goal");

        var aliceGet = await users.AliceClient.GetAsync(
            $"/api/goals/{goal.Id}");
        var bobGet = await users.BobClient.GetAsync(
            $"/api/goals/{goal.Id}");
        var bobUpdate = await users.BobClient.PutAsJsonAsync(
            $"/api/goals/{goal.Id}",
            CreateUpdateRequest("Bob update"));
        var bobComplete = await users.BobClient.PatchAsync(
            $"/api/goals/{goal.Id}/complete",
            content: null);
        var bobDelete = await users.BobClient.DeleteAsync(
            $"/api/goals/{goal.Id}");
        var missing = await users.AliceClient.GetAsync(
            "/api/goals/999999");

        Assert.Equal(HttpStatusCode.OK, aliceGet.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, bobGet.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, bobUpdate.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, bobComplete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, bobDelete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);

        var aliceUpdate = await users.AliceClient.PutAsJsonAsync(
            $"/api/goals/{goal.Id}",
            new
            {
                OwnerId = users.Bob.UserId,
                Title = "Alice updated goal",
                Category = "Ownership",
                Description = (string?)null,
                Priority = GoalPriority.Medium,
                Status = GoalStatus.InProgress,
                TargetDate = (DateTime?)null
            });
        var aliceComplete = await users.AliceClient.PatchAsync(
            $"/api/goals/{goal.Id}/complete",
            content: null);

        Assert.Equal(HttpStatusCode.NoContent, aliceUpdate.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, aliceComplete.StatusCode);

        var stored = await factory.FindGoalAsync(goal.Id);
        Assert.NotNull(stored);
        Assert.Equal(users.Alice.UserId, stored.OwnerId);
        Assert.Equal(GoalStatus.Completed, stored.Status);

        var aliceDelete = await users.AliceClient.DeleteAsync(
            $"/api/goals/{goal.Id}");

        Assert.Equal(HttpStatusCode.NoContent, aliceDelete.StatusCode);
        Assert.Null(await factory.FindGoalAsync(goal.Id));
    }

    [Fact]
    public async Task GoalActions_ShouldDeriveOwnershipThroughGoal()
    {
        var users = await CreateUsersAsync();
        var goal = await CreateGoalAsync(
            users.AliceClient,
            "Alice action goal");

        var create = await users.AliceClient.PostAsJsonAsync(
            $"/api/goals/{goal.Id}/actions",
            new CreateGoalActionRequest("Alice action"));
        var action = await create.Content
            .ReadFromJsonAsync<GoalActionDto>();

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        Assert.NotNull(action);

        var bobCreate = await users.BobClient.PostAsJsonAsync(
            $"/api/goals/{goal.Id}/actions",
            new CreateGoalActionRequest("Bob intrusion"));
        var bobRead = await users.BobClient.GetAsync(
            $"/api/goals/{goal.Id}/actions");
        var bobUpdate = await users.BobClient.PutAsJsonAsync(
            $"/api/goals/{goal.Id}/actions/{action.Id}",
            new UpdateGoalActionRequest("Bob update"));
        var bobComplete = await users.BobClient.PatchAsync(
            $"/api/goals/{goal.Id}/actions/{action.Id}/complete",
            content: null);
        var bobReopen = await users.BobClient.PatchAsync(
            $"/api/goals/{goal.Id}/actions/{action.Id}/reopen",
            content: null);
        var bobDelete = await users.BobClient.DeleteAsync(
            $"/api/goals/{goal.Id}/actions/{action.Id}");

        Assert.Equal(HttpStatusCode.NotFound, bobCreate.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, bobRead.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, bobUpdate.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, bobComplete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, bobReopen.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, bobDelete.StatusCode);

        var aliceUpdate = await users.AliceClient.PutAsJsonAsync(
            $"/api/goals/{goal.Id}/actions/{action.Id}",
            new UpdateGoalActionRequest("Alice updated action"));
        var aliceComplete = await users.AliceClient.PatchAsync(
            $"/api/goals/{goal.Id}/actions/{action.Id}/complete",
            content: null);
        var aliceReopen = await users.AliceClient.PatchAsync(
            $"/api/goals/{goal.Id}/actions/{action.Id}/reopen",
            content: null);
        var aliceRead = await users.AliceClient.GetFromJsonAsync<
            IReadOnlyList<GoalActionDto>>(
            $"/api/goals/{goal.Id}/actions");
        var aliceDelete = await users.AliceClient.DeleteAsync(
            $"/api/goals/{goal.Id}/actions/{action.Id}");

        Assert.Equal(HttpStatusCode.NoContent, aliceUpdate.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, aliceComplete.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, aliceReopen.StatusCode);
        Assert.NotNull(aliceRead);
        Assert.Single(aliceRead);
        Assert.Equal("Alice updated action", aliceRead[0].Title);
        Assert.Equal(HttpStatusCode.NoContent, aliceDelete.StatusCode);
    }

    [Fact]
    public async Task GoalEndpoints_ShouldRejectMissingAndInvalidJwt()
    {
        await factory.ResetDatabaseAsync();

        using var anonymousClient = factory.CreateClient();
        using var invalidClient = factory.CreateClient();
        invalidClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                "not-a-valid-jwt");

        var anonymous = await anonymousClient.GetAsync("/api/goals");
        var invalid = await invalidClient.GetAsync("/api/goals");

        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, invalid.StatusCode);
    }

    [Fact]
    public async Task LegacyNullOwnerGoal_ShouldNotBeVisibleToAuthenticatedUser()
    {
        var users = await CreateUsersAsync();
        await CreateGoalAsync(users.AliceClient, "Alice visible goal");
        await factory.AddLegacyGoalAsync();

        var result = await users.AliceClient.GetFromJsonAsync<
            PagedResult<GoalDto>>("/api/goals?pageSize=20");

        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Alice visible goal", result.Items[0].Title);
    }

    private async Task<Users> CreateUsersAsync()
    {
        await factory.ResetDatabaseAsync();

        using var registrationClient = factory.CreateClient();

        var alice = await factory.RegisterUserAsync(
            registrationClient,
            "alice@example.com");
        var bob = await factory.RegisterUserAsync(
            registrationClient,
            "bob@example.com");

        return new Users(
            alice,
            bob,
            factory.CreateAuthenticatedClient(alice),
            factory.CreateAuthenticatedClient(bob));
    }

    private static async Task<GoalDto> CreateGoalAsync(
        HttpClient client,
        string title,
        GoalPriority priority = GoalPriority.Medium)
    {
        var response = await client.PostAsJsonAsync(
            "/api/goals",
            new CreateGoalRequest(
                title,
                "Ownership",
                null,
                priority,
                null));

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<GoalDto>()
            ?? throw new InvalidOperationException(
                "Goal creation did not return a goal.");
    }

    private static UpdateGoalRequest CreateUpdateRequest(string title)
    {
        return new UpdateGoalRequest(
            title,
            "Ownership",
            null,
            GoalPriority.Medium,
            GoalStatus.InProgress,
            null);
    }

    private sealed record Users(
        AuthenticationResponse Alice,
        AuthenticationResponse Bob,
        HttpClient AliceClient,
        HttpClient BobClient);
}