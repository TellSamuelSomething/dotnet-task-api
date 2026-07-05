using TaskAPI.Models;

namespace TaskAPI.DTOs;

public class TaskQueryParams
{
    public bool? Completed { get; set; }
    public string? Search { get; set; }
    public Priority? Priority { get; set; }
    public DateTime? DueBefore { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
