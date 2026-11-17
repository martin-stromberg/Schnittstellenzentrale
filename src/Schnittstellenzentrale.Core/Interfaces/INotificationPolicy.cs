using Schnittstellenzentrale.Core.Enums;

namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>
/// Zentrale Auslösepolitik für SignalR-Änderungs-Benachrichtigungen.
/// Entscheidet an genau einer Stelle, ob und welche Benachrichtigung nach einem
/// mutierenden Vorgang gesendet wird: genau eine Nachricht pro Vorgang,
/// ausschließlich im Team-Speichermodus (bzw. immer bei Hintergrundvorgängen
/// ohne Anforderungskontext).
/// </summary>
public interface INotificationPolicy
{
    /// <summary>Löst genau eine Baum-Benachrichtigung aus (Anwendungen, Anwendungsgruppen, Import), sofern <paramref name="storageMode"/> Team ist.</summary>
    /// <param name="storageMode">Der Speichermodus des mutierenden Vorgangs.</param>
    Task NotifyTreeMutationAsync(StorageMode storageMode);

    /// <summary>Löst genau eine entitätsspezifische Benachrichtigung für einen Endpunkt aus, sofern <paramref name="storageMode"/> Team ist.</summary>
    /// <param name="storageMode">Der Speichermodus des mutierenden Vorgangs.</param>
    /// <param name="endpointId">ID des geänderten Endpunkts.</param>
    /// <param name="applicationId">ID der zugehörigen Anwendung.</param>
    Task NotifyEndpointMutationAsync(StorageMode storageMode, int endpointId, int applicationId);

    /// <summary>Löst genau eine entitätsspezifische Benachrichtigung für eine Endpunktgruppe aus, sofern <paramref name="storageMode"/> Team ist.</summary>
    /// <param name="storageMode">Der Speichermodus des mutierenden Vorgangs.</param>
    /// <param name="endpointGroupId">ID der geänderten Endpunktgruppe.</param>
    /// <param name="applicationId">ID der zugehörigen Anwendung.</param>
    Task NotifyEndpointGroupMutationAsync(StorageMode storageMode, int endpointGroupId, int applicationId);

    /// <summary>Löst genau eine Baum-Benachrichtigung für Hintergrundvorgänge ohne Anforderungskontext aus (z. B. Startabgleich freigegebener Systemdaten). Feuert immer.</summary>
    Task NotifyBackgroundMutationAsync();
}
