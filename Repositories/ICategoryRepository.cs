using TaskAPI.Models;

namespace TaskAPI.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync(string ownerId);
    Task<Category?> GetByIdAsync(int id, string ownerId);
    Task<Category> AddAsync(Category category);
    Task DeleteAsync(Category category);
}
