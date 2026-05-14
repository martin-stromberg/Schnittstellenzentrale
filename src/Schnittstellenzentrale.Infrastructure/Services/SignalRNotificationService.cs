using Microsoft.AspNetCore.SignalR;
using Schnittstellenzentrale.Core.Interfaces;

namespace Schnittstellenzentrale.Infrastructure.Services;

public class SignalRNotificationService<THub> : ISignalRNotificationService
    where THub : Hub
{
    private readonly IHubContext<THub> _hubContext;

    public SignalRNotificationService(IHubContext<THub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyApplicationChangedAsync(int applicationId)
    {
        await _hubContext.Clients.Group($"application:{applicationId}")
            .SendAsync("ApplicationChanged", applicationId);
    }

    public async Task NotifyGroupChangedAsync(int groupId)
    {
        await _hubContext.Clients.Group($"group:{groupId}")
            .SendAsync("GroupChanged", groupId);
    }
}
