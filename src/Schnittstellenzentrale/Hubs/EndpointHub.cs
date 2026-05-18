#pragma warning disable CS1591
using Microsoft.AspNetCore.SignalR;

namespace Schnittstellenzentrale.Hubs;

public class EndpointHub : Hub
{
    public async Task SubscribeToApplication(int applicationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"application:{applicationId}");
    }

    public async Task UnsubscribeFromApplication(int applicationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"application:{applicationId}");
    }

    public async Task SubscribeToGroup(int groupId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"group:{groupId}");
    }

    public async Task UnsubscribeFromGroup(int groupId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group:{groupId}");
    }
}
