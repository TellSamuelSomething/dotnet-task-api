using TaskAPI.DTOs;
using TaskAPI.Models;
using TaskAPI.Repositories;

namespace TaskAPI.Services;

public class CategoryService
{
    private readonly ICategoryRepository _repo;

    public CategoryService(ICategoryRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<CategoryResponse>> GetAllAsync(string ownerId)
    {
        var categories = await _repo.GetAllAsync(ownerId);
        return categories.Select(ToResponse).ToList();
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, string ownerId)
    {
        var category = new Category { Name = request.Name, OwnerId = ownerId };
        await _repo.AddAsync(category);
        return ToResponse(category);
    }

    public async Task<bool> DeleteAsync(int id, string ownerId)
    {
        var category = await _repo.GetByIdAsync(id, ownerId);
        if (category is null) return false;

        await _repo.DeleteAsync(category);
        return true;
    }

    public static CategoryResponse ToResponse(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name
    };
}
