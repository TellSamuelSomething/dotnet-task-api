using System.Diagnostics.Metrics;

namespace TaskAPI.Telemetry;

public class TaskMetrics
{
    public const string MeterName = "TaskAPI";

    private readonly Counter<int> _tasksCreated;
    private readonly Counter<int> _tasksCompleted;
    private readonly Counter<int> _tasksDeleted;
    private readonly Counter<int> _tasksRestored;

    public TaskMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);
        _tasksCreated   = meter.CreateCounter<int>("tasks.created",  "tasks", "Total tasks created");
        _tasksCompleted = meter.CreateCounter<int>("tasks.completed", "tasks", "Total tasks marked complete");
        _tasksDeleted   = meter.CreateCounter<int>("tasks.deleted",  "tasks", "Total tasks soft-deleted");
        _tasksRestored  = meter.CreateCounter<int>("tasks.restored", "tasks", "Total tasks restored from trash");
    }

    public void TaskCreated()   => _tasksCreated.Add(1);
    public void TaskCompleted() => _tasksCompleted.Add(1);
    public void TaskDeleted()   => _tasksDeleted.Add(1);
    public void TaskRestored()  => _tasksRestored.Add(1);
}
