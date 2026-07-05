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
        tasks = ApplySort(tasks, query.SortBy, query.Order);

        return await tasks
            .Include(t => t.Category)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(TaskQueryParams query, string ownerId) =>
        await BuildQuery(query, ownerId).CountAsync();

    public async Task<TaskItem?> GetByIdAsync(int id, string ownerId) =>
        await _db.Tasks
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == ownerId);

    public async Task<List<TaskItem>> GetOverdueAsync(string ownerId) =>
        await _db.Tasks
            .Include(t => t.Category)
            .Where(t => t.OwnerId == ownerId && !t.IsCompleted
                && t.DueDate.HasValue && t.DueDate.Value < DateTime.UtcNow)
            .OrderBy(t => t.DueDate)
            .ToListAsync();

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

        if (query.CategoryId.HasValue)
            tasks = tasks.Where(t => t.CategoryId == query.CategoryId.Value);

        return tasks;
    }

    private static IQueryable<TaskItem> ApplySort(IQueryable<TaskItem> tasks, string? sortBy, string order)
    {
        var descending = order.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return sortBy?.ToLower() switch
        {
            "title"     => descending ? tasks.OrderByDescending(t => t.Title)     : tasks.OrderBy(t => t.Title),
            "duedate"   => descending ? tasks.OrderByDescending(t => t.DueDate)   : tasks.OrderBy(t => t.DueDate),
            "priority"  => descending ? tasks.OrderByDescending(t => t.Priority)  : tasks.OrderBy(t => t.Priority),
            "createdat" => descending ? tasks.OrderByDescending(t => t.CreatedAt) : tasks.OrderBy(t => t.CreatedAt),
            _           => tasks.OrderBy(t => t.CreatedAt)
        };
    }
}
