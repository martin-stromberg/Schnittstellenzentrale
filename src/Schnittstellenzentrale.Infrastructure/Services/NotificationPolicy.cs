using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;

namespace Schnittstellenzentrale.Infrastructure.Services;

/// <summary>
/// Zentrale Auslösepolitik für SignalR-Änderungs-Benachrichtigungen.
/// Kapselt die Team-Modus-Entscheidung an einer einzigen Stelle und delegiert
/// an den unveränderten <see cref="ISignalRNotificationService"/>-Transport.
/// </summary>
public class NotificationPolicy : INotificationPolicy
{
    private readonly ISignalRNotificationService _notificationService;

    /// <summary>Initialisiert eine neue Instanz der <see cref="NotificationPolicy"/>.</summary>
    /// <param name="notificationService">Der SignalR-Benachrichtigungs-Transport.</param>
    public NotificationPolicy(ISignalRNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <inheritdoc/>
    public Task NotifyTreeMutationAsync(StorageMode storageMode)
        => storageMode == StorageMode.Team
            ? _notificationService.NotifyTreeChangedAsync()
            : Task.CompletedTask;

    /// <inheritdoc/>
    public Task NotifyEndpointMutationAsync(StorageMode storageMode, int endpointId, int applicationId)
        => storageMode == StorageMode.Team
            ? _notificationService.NotifyEndpointChangedAsync(endpointId, applicationId)
            : Task.CompletedTask;

    /// <inheritdoc/>
    public Task NotifyEndpointGroupMutationAsync(StorageMode storageMode, int endpointGroupId, int applicationId)
        => storageMode == StorageMode.Team
            ? _notificationService.NotifyEndpointGroupChangedAsync(endpointGroupId, applicationId)
            : Task.CompletedTask;

    /// <inheritdoc/>
    public Task NotifyBackgroundMutationAsync()
        => _notificationService.NotifyTreeChangedAsync();
}
