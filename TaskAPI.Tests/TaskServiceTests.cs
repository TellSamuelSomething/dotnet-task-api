using Microsoft.EntityFrameworkCore;
using TaskAPI.Data;
using TaskAPI.DTOs;
using TaskAPI.Services;

namespace TaskAPI.Tests;

public class TaskServiceTests
{
    private AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_ReturnsTaskWithCorrectTitle()
    {
        var db = CreateDb();
        var service = new TaskService(db);

        var result = await service.CreateAsync(new CreateTaskRequest
        {
            Title = "Test Task",
            Description = "Test Description"
        });

        Assert.Equal("Test Task", result.Title);
        Assert.Equal("Test Description", result.Description);
        Assert.False(result.IsCompleted);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyCompletedTasks_WhenFilterApplied()
    {
        var db = CreateDb();
        var service = new TaskService(db);

        await service.CreateAsync(new CreateTaskRequest { Title = "Incomplete Task" });
        var created = await service.CreateAsync(new CreateTaskRequest { Title = "Complete Task" });
        await service.UpdateAsync(created.Id, new UpdateTaskRequest
        {
            Title = created.Title,
            IsCompleted = true
        });

        var result = await service.GetAllAsync(new TaskQueryParams { Completed = true });

        Assert.Single(result.Items);
        Assert.Equal("Complete Task", result.Items[0].Title);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResults()
    {
        var db = CreateDb();
        var service = new TaskService(db);

        for (int i = 1; i <= 15; i++)
            await service.CreateAsync(new CreateTaskRequest { Title = $"Task {i}" });

        var result = await service.GetAllAsync(new TaskQueryParams { Page = 1, PageSize = 10 });

        Assert.Equal(10, result.Items.Count);
        Assert.Equal(15, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenTaskDoesNotExist()
    {
        var db = CreateDb();
        var service = new TaskService(db);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenTaskDoesNotExist()
    {
        var db = CreateDb();
        var service = new TaskService(db);

        var result = await service.DeleteAsync(999);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenTaskDoesNotExist()
    {
        var db = CreateDb();
        var service = new TaskService(db);

        var result = await service.UpdateAsync(999, new UpdateTaskRequest { Title = "Updated" });

        Assert.Null(result);
    }
}
