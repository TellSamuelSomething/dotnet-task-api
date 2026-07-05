using Microsoft.AspNetCore.SignalR;
using TaskAPI.Hubs;

namespace TaskAPI.Tests;

// No-op hub context for unit tests — all sends are discarded
internal class NullHubContext : IHubContext<TaskHub>
{
    public static readonly NullHubContext Instance = new();
    public IHubClients Clients => NullHubClients.Instance;
    public IGroupManager Groups => throw new NotSupportedException();
}

internal class NullHubClients : IHubClients
{
    public static readonly NullHubClients Instance = new();
    private static readonly IClientProxy NoOp = new NullClientProxy();

    public IClientProxy All => NoOp;
    public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => NoOp;
    public IClientProxy Client(string connectionId) => NoOp;
    public IClientProxy Clients(IReadOnlyList<string> connectionIds) => NoOp;
    public IClientProxy Group(string groupName) => NoOp;
    public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => NoOp;
    public IClientProxy Groups(IReadOnlyList<string> groupNames) => NoOp;
    public IClientProxy User(string userId) => NoOp;
    public IClientProxy Users(IReadOnlyList<string> userIds) => NoOp;
}

internal class NullClientProxy : IClientProxy
{
    public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
