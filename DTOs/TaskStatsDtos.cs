namespace TaskAPI.DTOs;

public class TaskStatsResponse
{
    public int Total { get; set; }
    public int Completed { get; set; }
    public int Incomplete { get; set; }
    public int Overdue { get; set; }
    public int HighPriority { get; set; }
    public int MediumPriority { get; set; }
    public int LowPriority { get; set; }
    public double CompletionRate { get; set; }
}
