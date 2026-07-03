using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TaskAPI.Data;
using TaskAPI.DTOs;
using TaskAPI.Services;

namespace TaskAPI.Tests;

public class TaskServiceTests
{
    private const string TestUser = "testuser";

    private static TaskService CreateService(AppDbContext db) =>
        new(db, NullLogger<TaskService>.Instance);

    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_ReturnsTaskWithCorrectTitle()
    {
        var service = CreateService(CreateDb());

        var result = await service.CreateAsync(new CreateTaskRequest
        {
            Title = "Test Task",
            Description = "Test Description"
        }, TestUser);

        Assert.Equal("Test Task", result.Title);
        Assert.Equal("Test Description", result.Description);
        Assert.False(result.IsCompleted);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyCompletedTasks_WhenFilterApplied()
    {
        var service = CreateService(CreateDb());

        await service.CreateAsync(new CreateTaskRequest { Title = "Incomplete Task" }, TestUser);
        var created = await service.CreateAsync(new CreateTaskRequest { Title = "Complete Task" }, TestUser);
        await service.UpdateAsync(created.Id, new UpdateTaskRequest
        {
            Title = created.Title,
            IsCompleted = true
        }, TestUser);

        var result = await service.GetAllAsync(new TaskQueryParams { Completed = true }, TestUser);

        Assert.Single(result.Items);
        Assert.Equal("Complete Task", result.Items[0].Title);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResults()
    {
        var service = CreateService(CreateDb());

        for (int i = 1; i <= 15; i++)
            await service.CreateAsync(new CreateTaskRequest { Title = $"Task {i}" }, TestUser);

        var result = await service.GetAllAsync(new TaskQueryParams { Page = 1, PageSize = 10 }, TestUser);

        Assert.Equal(10, result.Items.Count);
        Assert.Equal(15, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task GetAllAsync_DoesNotReturnOtherUsersTask()
    {
        var service = CreateService(CreateDb());

        await service.CreateAsync(new CreateTaskRequest { Title = "Other user task" }, "otheruser");

        var result = await service.GetAllAsync(new TaskQueryParams(), TestUser);

        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenTaskDoesNotExist()
    {
        var service = CreateService(CreateDb());

        var result = await service.GetByIdAsync(999, TestUser);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenTaskBelongsToOtherUser()
    {
        var service = CreateService(CreateDb());

        var created = await service.CreateAsync(new CreateTaskRequest { Title = "Someone else's task" }, "otheruser");

        var result = await service.GetByIdAsync(created.Id, TestUser);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenTaskDoesNotExist()
    {
        var service = CreateService(CreateDb());

        var result = await service.DeleteAsync(999, TestUser);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenTaskDoesNotExist()
    {
        var service = CreateService(CreateDb());

        var result = await service.UpdateAsync(999, new UpdateTaskRequest { Title = "Updated" }, TestUser);

        Assert.Null(result);
    }
}
