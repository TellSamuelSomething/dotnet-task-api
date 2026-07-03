using Microsoft.EntityFrameworkCore;
using TaskAPI.Data;
using TaskAPI.DTOs;
using TaskAPI.Models;

namespace TaskAPI.Services;

public class TaskService
{
    private readonly AppDbContext _db;
    private readonly ILogger<TaskService> _logger;

    public TaskService(AppDbContext db, ILogger<TaskService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<TaskResponse>> GetAllAsync(TaskQueryParams query, string ownerId)
    {
        _logger.LogInformation("Fetching tasks for user {OwnerId} - Page: {Page}, PageSize: {PageSize}, Completed: {Completed}, Search: {Search}",
            ownerId, query.Page, query.PageSize, query.Completed, query.Search);

        var tasks = _db.Tasks.Where(t => t.OwnerId == ownerId);

        if (query.Completed.HasValue)
            tasks = tasks.Where(t => t.IsCompleted == query.Completed.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
            tasks = tasks.Where(t => t.Title.Contains(query.Search));

        var totalCount = await tasks.CountAsync();

        var items = await tasks
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(t => ToResponse(t))
            .ToListAsync();

        return new PagedResult<TaskResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<TaskResponse?> GetByIdAsync(int id, string ownerId)
    {
        _logger.LogInformation("Fetching task {Id} for user {OwnerId}", id, ownerId);

        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == ownerId);

        if (task is null)
            _logger.LogWarning("Task {Id} not found for user {OwnerId}", id, ownerId);

        return task is null ? null : ToResponse(task);
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, string ownerId)
    {
        _logger.LogInformation("Creating task '{Title}' for user {OwnerId}", request.Title, ownerId);

        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            OwnerId = ownerId
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Task created with ID {Id}", task.Id);
        return ToResponse(task);
    }

    public async Task<TaskResponse?> UpdateAsync(int id, UpdateTaskRequest request, string ownerId)
    {
        _logger.LogInformation("Updating task {Id} for user {OwnerId}", id, ownerId);

        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == ownerId);
        if (task is null)
        {
            _logger.LogWarning("Task {Id} not found for user {OwnerId}", id, ownerId);
            return null;
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.IsCompleted = request.IsCompleted;

        await _db.SaveChangesAsync();
        return ToResponse(task);
    }

    public async Task<bool> DeleteAsync(int id, string ownerId)
    {
        _logger.LogInformation("Deleting task {Id} for user {OwnerId}", id, ownerId);

        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == ownerId);
        if (task is null)
        {
            _logger.LogWarning("Task {Id} not found for user {OwnerId}", id, ownerId);
            return false;
        }

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Task {Id} deleted", id);
        return true;
    }

    private static TaskResponse ToResponse(TaskItem t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        IsCompleted = t.IsCompleted,
        CreatedAt = t.CreatedAt
    };
}
