using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace TaskAPI.Hubs;

// Tells SignalR to use the same user identifier (Username) that our JWT uses
public class NameUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) =>
        connection.User?.FindFirstValue(ClaimTypes.Name);
}
