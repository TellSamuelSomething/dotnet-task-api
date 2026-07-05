using Microsoft.EntityFrameworkCore;
using TaskAPI.Data;
using TaskAPI.Models;

namespace TaskAPI.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;

    public CategoryRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Category>> GetAllAsync(string ownerId) =>
        await _db.Categories.Where(c => c.OwnerId == ownerId).ToListAsync();

    public async Task<Category?> GetByIdAsync(int id, string ownerId) =>
        await _db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == ownerId);

    public async Task<Category> AddAsync(Category category)
    {
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return category;
    }

    public async Task DeleteAsync(Category category)
    {
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
    }
}
