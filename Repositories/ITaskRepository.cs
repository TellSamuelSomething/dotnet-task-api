using TaskAPI.DTOs;
using TaskAPI.Models;

namespace TaskAPI.Repositories;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllAsync(TaskQueryParams query, string ownerId);
    Task<int> CountAsync(TaskQueryParams query, string ownerId);
    Task<TaskItem?> GetByIdAsync(int id, string ownerId);
    Task<List<TaskItem>> GetOverdueAsync(string ownerId);
    Task<List<TaskItem>> GetTrashAsync(string ownerId);
    Task<TaskItem?> GetByIdFromTrashAsync(int id, string ownerId);
    Task<TaskStatsResponse> GetStatsAsync(string ownerId);
    Task<TaskItem> AddAsync(TaskItem task);
    Task UpdateAsync(TaskItem task);
    Task SoftDeleteAsync(TaskItem task);
    Task DeleteAsync(TaskItem task);
}
