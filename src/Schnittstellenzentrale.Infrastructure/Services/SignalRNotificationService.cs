using Microsoft.AspNetCore.SignalR;
using Schnittstellenzentrale.Core.Interfaces;

namespace Schnittstellenzentrale.Infrastructure.Services;

/// <summary>
/// Sendet SignalR-Benachrichtigungen über den angegebenen Hub an verbundene Clients.
/// </summary>
public class SignalRNotificationService<THub> : ISignalRNotificationService
    where THub : Hub
{
    private readonly IHubContext<THub> _hubContext;

    /// <summary>
    /// Initialisiert eine neue Instanz des <see cref="SignalRNotificationService{THub}"/>.
    /// </summary>
    public SignalRNotificationService(IHubContext<THub> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// Benachrichtigt alle Clients im "workspace"-Channel über strukturelle Änderungen am Baum.
    /// </summary>
    public async Task NotifyTreeChangedAsync()
    {
        await _hubContext.Clients.Group("workspace").SendAsync("TreeChanged");
    }

    /// <summary>
    /// Benachrichtigt alle Clients in der Gruppe <c>application:{applicationId}</c> über eine Änderung der Anwendung.
    /// </summary>
    public async Task NotifyApplicationChangedAsync(int applicationId)
    {
        await _hubContext.Clients.Group($"application:{applicationId}")
            .SendAsync("ApplicationChanged", applicationId);
    }

    /// <summary>
    /// Benachrichtigt alle Clients in der Gruppe <c>group:{groupId}</c> über eine Änderung der Anwendungsgruppe.
    /// </summary>
    public async Task NotifyGroupChangedAsync(int groupId)
    {
        await _hubContext.Clients.Group($"group:{groupId}")
            .SendAsync("GroupChanged", groupId);
    }

    /// <summary>
    /// Sendet ein <c>EndpointChanged</c>-Event an die SignalR-Gruppe der Anwendung.
    /// </summary>
    public async Task NotifyEndpointChangedAsync(int endpointId, int applicationId)
    {
        await _hubContext.Clients.Group($"application:{applicationId}")
            .SendAsync("EndpointChanged", endpointId, applicationId);
    }

    /// <summary>
    /// Sendet ein <c>EndpointGroupChanged</c>-Event an die SignalR-Gruppe der Anwendung.
    /// </summary>
    public async Task NotifyEndpointGroupChangedAsync(int endpointGroupId, int applicationId)
    {
        await _hubContext.Clients.Group($"application:{applicationId}")
            .SendAsync("EndpointGroupChanged", endpointGroupId, applicationId);
    }

    /// <summary>
    /// Benachrichtigt alle Clients in der Gruppe <c>environments</c> über eine Änderung an Systemumgebungen.
    /// </summary>
    public async Task NotifyEnvironmentChangedAsync()
    {
        await _hubContext.Clients.Group("environments")
            .SendAsync("EnvironmentChanged");
    }
}
