using TaskAPI.DTOs;
using TaskAPI.Models;
using TaskAPI.Repositories;

namespace TaskAPI.Services;

public class TaskService
{
    private readonly ITaskRepository _repo;
    private readonly ILogger<TaskService> _logger;

    public TaskService(ITaskRepository repo, ILogger<TaskService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<PagedResult<TaskResponse>> GetAllAsync(TaskQueryParams query, string ownerId)
    {
        _logger.LogInformation("Fetching tasks for user {OwnerId} - Page: {Page}, PageSize: {PageSize}, Completed: {Completed}, Search: {Search}",
            ownerId, query.Page, query.PageSize, query.Completed, query.Search);

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
        _logger.LogInformation("Fetching task {Id} for user {OwnerId}", id, ownerId);

        var task = await _repo.GetByIdAsync(id, ownerId);

        if (task is null)
            _logger.LogWarning("Task {Id} not found for user {OwnerId}", id, ownerId);

        return task is null ? null : ToResponse(task);
    }

    public async Task<List<TaskResponse>> GetOverdueAsync(string ownerId)
    {
        _logger.LogInformation("Fetching overdue tasks for user {OwnerId}", ownerId);
        var tasks = await _repo.GetOverdueAsync(ownerId);
        return tasks.Select(ToResponse).ToList();
    }

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

        _logger.LogInformation("Task created with ID {Id}", task.Id);
        return ToResponse(task);
    }

    public async Task<TaskResponse?> UpdateAsync(int id, UpdateTaskRequest request, string ownerId)
    {
        _logger.LogInformation("Updating task {Id} for user {OwnerId}", id, ownerId);

        var task = await _repo.GetByIdAsync(id, ownerId);
        if (task is null)
        {
            _logger.LogWarning("Task {Id} not found for user {OwnerId}", id, ownerId);
            return null;
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.IsCompleted = request.IsCompleted;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.CategoryId = request.CategoryId;

        await _repo.UpdateAsync(task);
        return ToResponse(task);
    }

    public async Task<bool> DeleteAsync(int id, string ownerId)
    {
        _logger.LogInformation("Deleting task {Id} for user {OwnerId}", id, ownerId);

        var task = await _repo.GetByIdAsync(id, ownerId);
        if (task is null)
        {
            _logger.LogWarning("Task {Id} not found for user {OwnerId}", id, ownerId);
            return false;
        }

        await _repo.DeleteAsync(task);

        _logger.LogInformation("Task {Id} deleted", id);
        return true;
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
