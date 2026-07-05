using System.ComponentModel.DataAnnotations;

namespace TaskAPI.DTOs;

public class CreateCategoryRequest
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
}

public class CategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
