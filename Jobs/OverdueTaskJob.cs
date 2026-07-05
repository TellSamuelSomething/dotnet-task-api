using Microsoft.EntityFrameworkCore;
using TaskAPI.Data;

namespace TaskAPI.Jobs;

public class OverdueTaskJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OverdueTaskJob> _logger;

    public OverdueTaskJob(IServiceScopeFactory scopeFactory, ILogger<OverdueTaskJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task CheckOverdueTasksAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var overdueTasks = await db.Tasks
            .Where(t => !t.IsCompleted && t.DueDate.HasValue && t.DueDate.Value < DateTime.UtcNow)
            .GroupBy(t => t.OwnerId)
            .Select(g => new { OwnerId = g.Key, Count = g.Count() })
            .ToListAsync();

        if (overdueTasks.Count == 0)
        {
            _logger.LogInformation("Overdue task check: no overdue tasks found.");
            return;
        }

        foreach (var entry in overdueTasks)
            _logger.LogWarning("User {OwnerId} has {Count} overdue task(s).", entry.OwnerId, entry.Count);
    }
}
