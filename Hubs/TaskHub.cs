using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TaskAPI.Hubs;

[Authorize]
public class TaskHub : Hub
{
    // Clients connect to /hubs/tasks and receive push events:
    // "TaskCreated", "TaskUpdated", "TaskDeleted"
}
