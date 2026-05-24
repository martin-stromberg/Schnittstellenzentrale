using Microsoft.AspNetCore.SignalR;

namespace Schnittstellenzentrale.Hubs;

/// <summary>SignalR-Hub für Echtzeit-Benachrichtigungen über Endpunkt- und Umgebungsänderungen.</summary>
public class EndpointHub : Hub
{
    /// <summary>Trägt den aktuellen Client in die SignalR-Gruppe der angegebenen Anwendung ein.</summary>
    public async Task SubscribeToApplication(int applicationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"application:{applicationId}");
    }

    /// <summary>Entfernt den aktuellen Client aus der SignalR-Gruppe der angegebenen Anwendung.</summary>
    public async Task UnsubscribeFromApplication(int applicationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"application:{applicationId}");
    }

    /// <summary>Trägt den aktuellen Client in die SignalR-Gruppe der angegebenen Anwendungsgruppe ein.</summary>
    public async Task SubscribeToGroup(int groupId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"group:{groupId}");
    }

    /// <summary>Entfernt den aktuellen Client aus der SignalR-Gruppe der angegebenen Anwendungsgruppe.</summary>
    public async Task UnsubscribeFromGroup(int groupId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group:{groupId}");
    }

    /// <summary>Trägt den aktuellen Client in die SignalR-Gruppe <c>environments</c> ein.</summary>
    public async Task SubscribeToEnvironments()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "environments");
    }

    /// <summary>Entfernt den aktuellen Client aus der SignalR-Gruppe <c>environments</c>.</summary>
    public async Task UnsubscribeFromEnvironments()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "environments");
    }
}
