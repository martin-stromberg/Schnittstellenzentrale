namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>
/// Sendet SignalR-Benachrichtigungen über Änderungen an Anwendungen, Gruppen, Endpunkten und Endpunktgruppen.
/// </summary>
public interface ISignalRNotificationService
{
    /// <summary>Benachrichtigt Clients über Änderungen an einer Anwendung.</summary>
    Task NotifyApplicationChangedAsync(int applicationId);

    /// <summary>Benachrichtigt Clients über Änderungen an einer Anwendungsgruppe.</summary>
    Task NotifyGroupChangedAsync(int groupId);

    /// <summary>Benachrichtigt Clients über Änderungen an einem Endpunkt; <paramref name="applicationId"/> bestimmt die SignalR-Gruppe.</summary>
    Task NotifyEndpointChangedAsync(int endpointId, int applicationId);

    /// <summary>Benachrichtigt Clients über Änderungen an einer Endpunktgruppe; <paramref name="applicationId"/> bestimmt die SignalR-Gruppe.</summary>
    Task NotifyEndpointGroupChangedAsync(int endpointGroupId, int applicationId);

    /// <summary>Benachrichtigt Clients über Änderungen an Systemumgebungen (nur im Team-Modus).</summary>
    Task NotifyEnvironmentChangedAsync();
}
