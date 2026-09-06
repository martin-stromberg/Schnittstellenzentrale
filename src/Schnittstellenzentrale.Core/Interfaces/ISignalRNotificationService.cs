namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>
/// Sendet SignalR-Benachrichtigungen über Änderungen an Anwendungen, Gruppen, Endpunkten und Endpunktgruppen.
/// </summary>
public interface ISignalRNotificationService
{
    /// <summary>Benachrichtigt alle Clients im "workspace"-Channel über strukturelle Änderungen am Baum (App/Gruppe angelegt, umbenannt, gelöscht).</summary>
    Task NotifyTreeChangedAsync();

    /// <summary>Benachrichtigt Clients über Änderungen an einer Anwendung.</summary>
    /// <param name="applicationId">ID der geänderten Anwendung.</param>
    Task NotifyApplicationChangedAsync(int applicationId);

    /// <summary>Benachrichtigt Clients über Änderungen an einer Anwendungsgruppe.</summary>
    /// <param name="groupId">ID der geänderten Anwendungsgruppe.</param>
    Task NotifyGroupChangedAsync(int groupId);

    /// <summary>Benachrichtigt Clients über Änderungen an einem Endpunkt; <paramref name="applicationId"/> bestimmt die SignalR-Gruppe.</summary>
    /// <param name="endpointId">ID des geänderten Endpunkts.</param>
    /// <param name="applicationId">ID der zugehörigen Anwendung.</param>
    Task NotifyEndpointChangedAsync(int endpointId, int applicationId);

    /// <summary>Benachrichtigt Clients über Änderungen an einer Endpunktgruppe; <paramref name="applicationId"/> bestimmt die SignalR-Gruppe.</summary>
    /// <param name="endpointGroupId">ID der geänderten Endpunktgruppe.</param>
    /// <param name="applicationId">ID der zugehörigen Anwendung.</param>
    Task NotifyEndpointGroupChangedAsync(int endpointGroupId, int applicationId);

    /// <summary>Benachrichtigt Clients über Änderungen an Systemumgebungen (nur im Team-Modus).</summary>
    Task NotifyEnvironmentChangedAsync();
}
