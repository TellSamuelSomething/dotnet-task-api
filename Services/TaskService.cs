using Microsoft.AspNetCore.SignalR;
using TaskAPI.DTOs;
using TaskAPI.Hubs;
using TaskAPI.Models;
using TaskAPI.Repositories;
using TaskAPI.Telemetry;

namespace TaskAPI.Services;

public class TaskService
{
    private readonly ITaskRepository _repo;
    private readonly ILogger<TaskService> _logger;
    private readonly IHubContext<TaskHub> _hub;
    private readonly TaskMetrics _metrics;

    public TaskService(ITaskRepository repo, ILogger<TaskService> logger, IHubContext<TaskHub> hub, TaskMetrics metrics)
    {
        _repo = repo;
        _logger = logger;
        _hub = hub;
        _metrics = metrics;
    }

    public async Task<PagedResult<TaskResponse>> GetAllAsync(TaskQueryParams query, string ownerId)
    {
        _logger.LogInformation("Fetching tasks for user {OwnerId} - Page: {Page}, PageSize: {PageSize}",
            ownerId, query.Page, query.PageSize);

        var items = await _repo.GetAllAsync(query, ownerId);
        var totalCount = await _repo.CountAsync(query, ownerId);

        return new PagedResult<TaskResponse>
        {
            Items = items.Select(ToResponse).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<TaskResponse?> GetByIdAsync(int id, string ownerId)
    {
        var task = await _repo.GetByIdAsync(id, ownerId);
        if (task is null) _logger.LogWarning("Task {Id} not found for user {OwnerId}", id, ownerId);
        return task is null ? null : ToResponse(task);
    }

    public async Task<List<TaskResponse>> GetOverdueAsync(string ownerId)
    {
        var tasks = await _repo.GetOverdueAsync(ownerId);
        return tasks.Select(ToResponse).ToList();
    }

    public async Task<List<TaskResponse>> GetTrashAsync(string ownerId)
    {
        var tasks = await _repo.GetTrashAsync(ownerId);
        return tasks.Select(ToResponse).ToList();
    }

    public async Task<TaskStatsResponse> GetStatsAsync(string ownerId) =>
        await _repo.GetStatsAsync(ownerId);

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, string ownerId)
    {
        _logger.LogInformation("Creating task '{Title}' for user {OwnerId}", request.Title, ownerId);

        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            DueDate = request.DueDate,
            CategoryId = request.CategoryId,
            OwnerId = ownerId
        };

        await _repo.AddAsync(task);
        _metrics.TaskCreated();

        var response = ToResponse(task);
        await _hub.Clients.User(ownerId).SendAsync("TaskCreated", response);
        return response;
    }

    public async Task<TaskResponse?> UpdateAsync(int id, UpdateTaskRequest request, string ownerId)
    {
        var task = await _repo.GetByIdAsync(id, ownerId);
        if (task is null) return null;

        var wasCompleted = task.IsCompleted;

        task.Title = request.Title;
        task.Description = request.Description;
        task.IsCompleted = request.IsCompleted;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.CategoryId = request.CategoryId;

        await _repo.UpdateAsync(task);

        if (!wasCompleted && task.IsCompleted)
            _metrics.TaskCompleted();

        var response = ToResponse(task);
        await _hub.Clients.User(ownerId).SendAsync("TaskUpdated", response);
        return response;
    }

    public async Task<bool> DeleteAsync(int id, string ownerId)
    {
        var task = await _repo.GetByIdAsync(id, ownerId);
        if (task is null) return false;

        await _repo.SoftDeleteAsync(task);
        _metrics.TaskDeleted();

        await _hub.Clients.User(ownerId).SendAsync("TaskDeleted", new { id });
        return true;
    }

    public async Task<TaskResponse?> RestoreAsync(int id, string ownerId)
    {
        var task = await _repo.GetByIdFromTrashAsync(id, ownerId);
        if (task is null) return null;

        task.DeletedAt = null;
        await _repo.UpdateAsync(task);
        _metrics.TaskRestored();

        var response = ToResponse(task);
        await _hub.Clients.User(ownerId).SendAsync("TaskRestored", response);
        return response;
    }

    private static TaskResponse ToResponse(TaskItem t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        IsCompleted = t.IsCompleted,
        Priority = t.Priority,
        DueDate = t.DueDate,
        CreatedAt = t.CreatedAt,
        Category = t.Category is null ? null : CategoryService.ToResponse(t.Category)
    };
}
