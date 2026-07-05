using Microsoft.EntityFrameworkCore;
using TaskAPI.Data;
using TaskAPI.DTOs;
using TaskAPI.Models;

namespace TaskAPI.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _db;

    public TaskRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<TaskItem>> GetAllAsync(TaskQueryParams query, string ownerId)
    {
        var tasks = BuildQuery(query, ownerId);

        return await tasks
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(TaskQueryParams query, string ownerId) =>
        await BuildQuery(query, ownerId).CountAsync();

    public async Task<TaskItem?> GetByIdAsync(int id, string ownerId) =>
        await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == ownerId);

    public async Task<TaskItem> AddAsync(TaskItem task)
    {
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
        return task;
    }

    public async Task UpdateAsync(TaskItem task)
    {
        _db.Tasks.Update(task);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(TaskItem task)
    {
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
    }

    private IQueryable<TaskItem> BuildQuery(TaskQueryParams query, string ownerId)
    {
        var tasks = _db.Tasks.Where(t => t.OwnerId == ownerId);

        if (query.Completed.HasValue)
            tasks = tasks.Where(t => t.IsCompleted == query.Completed.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
            tasks = tasks.Where(t => t.Title.Contains(query.Search));

        if (query.Priority.HasValue)
            tasks = tasks.Where(t => t.Priority == query.Priority.Value);

        if (query.DueBefore.HasValue)
            tasks = tasks.Where(t => t.DueDate.HasValue && t.DueDate.Value <= query.DueBefore.Value);

        return tasks;
    }
}
