using System.Net;
using System.Net.Http.Json;

using Elevating.Api.IntegrationTests.Infrastructure;
using Elevating.Application.Common.Pagination;
using Elevating.Application.DTOs.Goals;
using Elevating.Domain.Enums;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Elevating.Api.IntegrationTests.Controllers;

public sealed class GoalsControllerTests
    : IClassFixture<ElevatingApiFactory>
{
    private readonly ElevatingApiFactory factory;
    private readonly HttpClient client;

    public GoalsControllerTests(
        ElevatingApiFactory factory)
    {
        this.factory = factory;

        client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
    }

    [Fact]
    public async Task GetHealth_ShouldReturnOk()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        // Act
        var response = await client.GetAsync("/api/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ShouldReturnPaginatedGoals()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        // Act
        var response = await client.GetAsync(
            "/api/goals?pageNumber=1&pageSize=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<PagedResult<GoalDto>>();

        Assert.NotNull(result);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(2, result.Items.Count);
        Assert.False(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public async Task GetAll_WithFiltering_ShouldReturnMatchingGoals()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        // Act
        var response = await client.GetAsync(
            "/api/goals?category=Development&status=Completed");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<PagedResult<GoalDto>>();

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);

        Assert.All(
            result.Items,
            goal =>
            {
                Assert.Equal("Development", goal.Category);
                Assert.Equal(GoalStatus.Completed, goal.Status);
            });
    }

    [Fact]
    public async Task GetAll_WithSorting_ShouldReturnGoalsInRequestedOrder()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        // Act
        var response = await client.GetAsync(
            "/api/goals" +
            "?sortBy=Title" +
            "&sortDirection=Ascending" +
            "&pageSize=20");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<PagedResult<GoalDto>>();

        Assert.NotNull(result);

        var titles = result.Items
            .Select(goal => goal.Title)
            .ToList();

        var expectedTitles = titles
            .OrderBy(title => title)
            .ToList();

        Assert.Equal(expectedTitles, titles);
    }

    [Fact]
    public async Task GetSummary_ShouldReturnGoalTotals()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        // Act
        var response = await client.GetAsync(
            "/api/goals/summary");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var summary =
            await response.Content
                .ReadFromJsonAsync<GoalSummaryDto>();

        Assert.NotNull(summary);
        Assert.Equal(5, summary.Total);
        Assert.Equal(2, summary.NotStarted);
        Assert.Equal(1, summary.InProgress);
        Assert.Equal(2, summary.Completed);
        Assert.Equal(0, summary.Overdue);
    }

    [Fact]
    public async Task GetSummary_ShouldCountOnlyOverdueIncompleteGoals()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        var today = DateTime.UtcNow.Date;

        await factory.AddGoalAsync(
            title: "Overdue not started goal",
            status: GoalStatus.NotStarted,
            targetDate: today.AddDays(-2));

        await factory.AddGoalAsync(
            title: "Overdue in-progress goal",
            status: GoalStatus.InProgress,
            targetDate: today.AddDays(-1));

        await factory.AddGoalAsync(
            title: "Completed past-due goal",
            status: GoalStatus.Completed,
            targetDate: today.AddDays(-3));

        await factory.AddGoalAsync(
            title: "Goal due today",
            status: GoalStatus.NotStarted,
            targetDate: today);

        await factory.AddGoalAsync(
            title: "Goal without target date",
            status: GoalStatus.InProgress,
            targetDate: null,
            useDefaultTargetDate: false);

        // Act
        var response = await client.GetAsync(
            "/api/goals/summary");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var summary =
            await response.Content
                .ReadFromJsonAsync<GoalSummaryDto>();

        Assert.NotNull(summary);

        Assert.Equal(10, summary.Total);
        Assert.Equal(4, summary.NotStarted);
        Assert.Equal(3, summary.InProgress);
        Assert.Equal(3, summary.Completed);
        Assert.Equal(2, summary.Overdue);
    }

    [Fact]
    public async Task GetById_WhenGoalExists_ShouldReturnGoal()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        var id = await factory.AddGoalAsync(
            title: "Retrieve this goal");

        // Act
        var response = await client.GetAsync(
            $"/api/goals/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var goal =
            await response.Content
                .ReadFromJsonAsync<GoalDto>();

        Assert.NotNull(goal);
        Assert.Equal(id, goal.Id);
        Assert.Equal("Retrieve this goal", goal.Title);
    }

    [Fact]
    public async Task GetById_WhenGoalDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        // Act
        var response = await client.GetAsync(
            "/api/goals/999999");

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task Create_WithValidRequest_ShouldReturnCreatedGoal()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        var request = new CreateGoalRequest(
            Title: "Create Angular dashboard",
            Category: "Frontend",
            Description: "Build the main goals dashboard.",
            Priority: GoalPriority.High,
            TargetDate: DateTime.UtcNow.AddDays(15));

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/goals",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        Assert.NotNull(response.Headers.Location);

        var createdGoal =
            await response.Content
                .ReadFromJsonAsync<GoalDto>();

        Assert.NotNull(createdGoal);
        Assert.True(createdGoal.Id > 0);
        Assert.Equal(
            "Create Angular dashboard",
            createdGoal.Title);

        Assert.Equal(
            GoalStatus.NotStarted,
            createdGoal.Status);

        var storedGoal =
            await factory.FindGoalAsync(createdGoal.Id);

        Assert.NotNull(storedGoal);
        Assert.Equal(
            "Create Angular dashboard",
            storedGoal.Title);
    }

    [Fact]
    public async Task Create_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        var request = new CreateGoalRequest(
            Title: string.Empty,
            Category: string.Empty,
            Description: null,
            Priority: GoalPriority.High,
            TargetDate: null);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/goals",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var problem =
            await response.Content
                .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.NotEmpty(problem.Errors);
    }

    [Fact]
    public async Task Update_WhenGoalExists_ShouldReturnNoContentAndUpdateDatabase()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        var id = await factory.AddGoalAsync(
            title: "Original title");

        var request = new UpdateGoalRequest(
            Title: "Updated title",
            Category: "Development",
            Description: "Updated through the API.",
            Priority: GoalPriority.High,
            Status: GoalStatus.InProgress,
            TargetDate: DateTime.UtcNow.AddDays(8));

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/goals/{id}",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);

        var storedGoal =
            await factory.FindGoalAsync(id);

        Assert.NotNull(storedGoal);
        Assert.Equal("Updated title", storedGoal.Title);
        Assert.Equal(
            GoalStatus.InProgress,
            storedGoal.Status);
        Assert.Equal(
            GoalPriority.High,
            storedGoal.Priority);
    }

    [Fact]
    public async Task Update_WhenGoalDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        var request = new UpdateGoalRequest(
            Title: "Missing goal",
            Category: "Testing",
            Description: null,
            Priority: GoalPriority.Low,
            Status: GoalStatus.NotStarted,
            TargetDate: null);

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/goals/999999",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task Complete_WhenGoalExists_ShouldMarkGoalCompleted()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        var id = await factory.AddGoalAsync(
            title: "Complete this goal",
            status: GoalStatus.InProgress);

        // Act
        var response = await client.PatchAsync(
            $"/api/goals/{id}/complete",
            content: null);

        // Assert
        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);

        var storedGoal =
            await factory.FindGoalAsync(id);

        Assert.NotNull(storedGoal);
        Assert.Equal(
            GoalStatus.Completed,
            storedGoal.Status);
    }

    [Fact]
    public async Task Delete_WhenGoalExists_ShouldRemoveGoal()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        var id = await factory.AddGoalAsync(
            title: "Delete this goal");

        // Act
        var response = await client.DeleteAsync(
            $"/api/goals/{id}");

        // Assert
        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);

        var storedGoal =
            await factory.FindGoalAsync(id);

        Assert.Null(storedGoal);
    }

    [Fact]
    public async Task Delete_WhenGoalDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        await factory.ResetDatabaseAsync();

        // Act
        var response = await client.DeleteAsync(
            "/api/goals/999999");

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }
}