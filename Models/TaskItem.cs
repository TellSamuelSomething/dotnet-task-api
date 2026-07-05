namespace TaskAPI.Models;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; } = false;
    public Priority Priority { get; set; } = Priority.Medium;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string OwnerId { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    public DateTime? DeletedAt { get; set; }
}
