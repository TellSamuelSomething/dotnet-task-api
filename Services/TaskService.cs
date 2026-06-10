using Microsoft.EntityFrameworkCore;
using TaskAPI.Data;
using TaskAPI.DTOs;
using TaskAPI.Models;

namespace TaskAPI.Services;

public class TaskService
{
    private readonly AppDbContext _db;

    public TaskService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<TaskResponse>> GetAllAsync()
    {
        return await _db.Tasks
            .Select(t => ToResponse(t))
            .ToListAsync();
    }

    public async Task<TaskResponse?> GetByIdAsync(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        return task is null ? null : ToResponse(task);
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request)
    {
        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
        return ToResponse(task);
    }

    public async Task<TaskResponse?> UpdateAsync(int id, UpdateTaskRequest request)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task is null) return null;

        task.Title = request.Title;
        task.Description = request.Description;
        task.IsCompleted = request.IsCompleted;

        await _db.SaveChangesAsync();
        return ToResponse(task);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task is null) return false;

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
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
