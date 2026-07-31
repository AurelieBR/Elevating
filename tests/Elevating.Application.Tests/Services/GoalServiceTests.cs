using Elevating.Application.Common.Queries;
using Elevating.Application.DTOs.Goals;
using Elevating.Application.Interfaces.Repositories;
using Elevating.Application.Services;
using Elevating.Domain.Entities;
using Elevating.Domain.Enums;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

namespace Elevating.Application.Tests.Services;

public sealed class GoalServiceTests
{
    private readonly Mock<IGoalRepository> goalRepositoryMock;
    private readonly GoalService goalService;

    public GoalServiceTests()
    {
        goalRepositoryMock = new Mock<IGoalRepository>();

        goalService = new GoalService(
            goalRepositoryMock.Object,
            NullLogger<GoalService>.Instance);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnMappedPagedResult()
    {
        // Arrange
        var parameters = new GoalQueryParameters
        {
            PageNumber = 2,
            PageSize = 2,
            Status = GoalStatus.InProgress,
            SortBy = GoalSortBy.Title,
            SortDirection = SortDirection.Ascending
        };

        var goals = new List<Goal>
        {
            CreateGoal(
                id: 3,
                title: "Build Angular client",
                status: GoalStatus.InProgress),

            CreateGoal(
                id: 4,
                title: "Write unit tests",
                status: GoalStatus.InProgress)
        };

        goalRepositoryMock
            .Setup(repository => repository.GetPagedAsync(
                parameters,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((goals, 6));

        // Act
        var result = await goalService.GetPagedAsync(parameters);

        // Assert
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(6, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(3, result.Items[0].Id);
        Assert.Equal("Build Angular client", result.Items[0].Title);
        Assert.Equal(GoalStatus.InProgress, result.Items[0].Status);

        goalRepositoryMock.Verify(
            repository => repository.GetPagedAsync(
                parameters,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldReturnMappedSummary()
    {
        // Arrange
        var repositoryResult = new GoalSummaryResult(
            Total: 12,
            NotStarted: 4,
            InProgress: 3,
            Completed: 5,
            Overdue: 2);

        goalRepositoryMock
            .Setup(repository => repository.GetSummaryAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await goalService.GetSummaryAsync();

        // Assert
        Assert.Equal(12, result.Total);
        Assert.Equal(4, result.NotStarted);
        Assert.Equal(3, result.InProgress);
        Assert.Equal(5, result.Completed);
        Assert.Equal(2, result.Overdue);

        goalRepositoryMock.Verify(
            repository => repository.GetSummaryAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenGoalExists_ShouldReturnMappedGoal()
    {
        // Arrange
        var goal = CreateGoal(
            id: 10,
            title: "Complete backend");

        goalRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(goal);

        // Act
        var result = await goalService.GetByIdAsync(10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(goal.Id, result.Id);
        Assert.Equal(goal.Title, result.Title);
        Assert.Equal(goal.Category, result.Category);
        Assert.Equal(goal.Description, result.Description);
        Assert.Equal(goal.Priority, result.Priority);
        Assert.Equal(goal.Status, result.Status);
    }

    [Fact]
    public async Task GetByIdAsync_WhenGoalDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        goalRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                999,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Goal?)null);

        // Act
        var result = await goalService.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateGoalAndReturnMappedDto()
    {
        // Arrange
        var request = new CreateGoalRequest(
            Title: "  Learn Angular  ",
            Category: "  Development  ",
            Description: "  Build the frontend application.  ",
            Priority: GoalPriority.High,
            TargetDate: new DateTime(2026, 9, 1));

        Goal? capturedGoal = null;

        goalRepositoryMock
            .Setup(repository => repository.AddAsync(
                It.IsAny<Goal>(),
                It.IsAny<CancellationToken>()))
            .Callback<Goal, CancellationToken>(
                (goal, _) =>
                {
                    capturedGoal = goal;
                    goal.Id = 25;
                })
            .Returns(Task.CompletedTask);

        goalRepositoryMock
            .Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await goalService.CreateAsync(request);

        // Assert
        Assert.NotNull(capturedGoal);

        Assert.Equal("Learn Angular", capturedGoal.Title);
        Assert.Equal("Development", capturedGoal.Category);
        Assert.Equal(
            "Build the frontend application.",
            capturedGoal.Description);

        Assert.Equal(GoalPriority.High, capturedGoal.Priority);
        Assert.Equal(GoalStatus.NotStarted, capturedGoal.Status);
        Assert.Equal(request.TargetDate, capturedGoal.TargetDate);

        Assert.NotEqual(default, capturedGoal.CreatedDate);
        Assert.Equal(
            capturedGoal.CreatedDate,
            capturedGoal.UpdatedDate);

        Assert.Equal(25, result.Id);
        Assert.Equal("Learn Angular", result.Title);
        Assert.Equal(GoalStatus.NotStarted, result.Status);

        goalRepositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<Goal>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        goalRepositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenDescriptionIsWhitespace_ShouldStoreNull()
    {
        // Arrange
        var request = new CreateGoalRequest(
            Title: "New goal",
            Category: "Personal",
            Description: "   ",
            Priority: GoalPriority.Medium,
            TargetDate: null);

        Goal? capturedGoal = null;

        goalRepositoryMock
            .Setup(repository => repository.AddAsync(
                It.IsAny<Goal>(),
                It.IsAny<CancellationToken>()))
            .Callback<Goal, CancellationToken>(
                (goal, _) => capturedGoal = goal)
            .Returns(Task.CompletedTask);

        goalRepositoryMock
            .Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await goalService.CreateAsync(request);

        // Assert
        Assert.NotNull(capturedGoal);
        Assert.Null(capturedGoal.Description);
    }

    [Fact]
    public async Task UpdateAsync_WhenGoalExists_ShouldUpdateAndSaveGoal()
    {
        // Arrange
        var originalUpdatedDate = DateTime.UtcNow.AddDays(-2);

        var goal = CreateGoal(
            id: 12,
            title: "Old title",
            status: GoalStatus.NotStarted);

        goal.UpdatedDate = originalUpdatedDate;

        var request = new UpdateGoalRequest(
            Title: "  Updated title  ",
            Category: "  Testing  ",
            Description: "  Updated description  ",
            Priority: GoalPriority.High,
            Status: GoalStatus.InProgress,
            TargetDate: new DateTime(2026, 10, 15));

        goalRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                12,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(goal);

        goalRepositoryMock
            .Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await goalService.UpdateAsync(12, request);

        // Assert
        Assert.True(result);

        Assert.Equal("Updated title", goal.Title);
        Assert.Equal("Testing", goal.Category);
        Assert.Equal("Updated description", goal.Description);
        Assert.Equal(GoalPriority.High, goal.Priority);
        Assert.Equal(GoalStatus.InProgress, goal.Status);
        Assert.Equal(request.TargetDate, goal.TargetDate);
        Assert.True(goal.UpdatedDate > originalUpdatedDate);

        goalRepositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenGoalDoesNotExist_ShouldReturnFalseWithoutSaving()
    {
        // Arrange
        var request = new UpdateGoalRequest(
            Title: "Updated title",
            Category: "Testing",
            Description: null,
            Priority: GoalPriority.High,
            Status: GoalStatus.InProgress,
            TargetDate: null);

        goalRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                999,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Goal?)null);

        // Act
        var result = await goalService.UpdateAsync(999, request);

        // Assert
        Assert.False(result);

        goalRepositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CompleteAsync_WhenGoalExists_ShouldMarkGoalCompleted()
    {
        // Arrange
        var originalUpdatedDate = DateTime.UtcNow.AddDays(-2);

        var goal = CreateGoal(
            id: 7,
            title: "Write tests",
            status: GoalStatus.InProgress);

        goal.UpdatedDate = originalUpdatedDate;

        goalRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                7,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(goal);

        goalRepositoryMock
            .Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await goalService.CompleteAsync(7);

        // Assert
        Assert.True(result);
        Assert.Equal(GoalStatus.Completed, goal.Status);
        Assert.True(goal.UpdatedDate > originalUpdatedDate);

        goalRepositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_WhenGoalDoesNotExist_ShouldReturnFalseWithoutSaving()
    {
        // Arrange
        goalRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                999,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Goal?)null);

        // Act
        var result = await goalService.CompleteAsync(999);

        // Assert
        Assert.False(result);

        goalRepositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenGoalExists_ShouldRemoveAndSaveGoal()
    {
        // Arrange
        var goal = CreateGoal(
            id: 15,
            title: "Delete me");

        goalRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                15,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(goal);

        goalRepositoryMock
            .Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await goalService.DeleteAsync(15);

        // Assert
        Assert.True(result);

        goalRepositoryMock.Verify(
            repository => repository.Remove(goal),
            Times.Once);

        goalRepositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenGoalDoesNotExist_ShouldReturnFalseWithoutRemoving()
    {
        // Arrange
        goalRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                999,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Goal?)null);

        // Act
        var result = await goalService.DeleteAsync(999);

        // Assert
        Assert.False(result);

        goalRepositoryMock.Verify(
            repository => repository.Remove(It.IsAny<Goal>()),
            Times.Never);

        goalRepositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Goal CreateGoal(
        int id,
        string title,
        GoalStatus status = GoalStatus.NotStarted)
    {
        var createdDate = DateTime.UtcNow.AddDays(-5);

        return new Goal
        {
            Id = id,
            Title = title,
            Category = "Development",
            Description = "Test goal description",
            Priority = GoalPriority.Medium,
            Status = status,
            TargetDate = DateTime.UtcNow.AddDays(10),
            CreatedDate = createdDate,
            UpdatedDate = createdDate
        };
    }

    [Fact]
    public async Task GetPagedAsync_WhenParametersAreNull_ShouldThrow()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => goalService.GetPagedAsync(null!));
    }

    [Fact]
    public async Task CreateAsync_WhenRequestIsNull_ShouldThrow()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => goalService.CreateAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_WhenRequestIsNull_ShouldThrow()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => goalService.UpdateAsync(1, null!));
    }
}