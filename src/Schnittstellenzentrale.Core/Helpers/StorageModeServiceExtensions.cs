using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;

namespace Schnittstellenzentrale.Core.Helpers;

/// <summary>Erweiterungsmethoden für <see cref="IStorageModeService"/>.</summary>
public static class StorageModeServiceExtensions
{
    /// <summary>Ermittelt den aktuellen Eigentümer basierend auf dem gewählten Speichermodus.</summary>
    /// <param name="storageModeService">Der Speichermodus-Service.</param>
    /// <param name="currentUserService">Der aktuelle Benutzer-Service.</param>
    /// <returns>Der Benutzername oder <c>null</c> bei globalem Modus.</returns>
    public static string? GetCurrentOwner(this IStorageModeService storageModeService, ICurrentUserService currentUserService)
    {
        return storageModeService.CurrentMode == StorageMode.User
            ? currentUserService.GetCurrentUserName()
            : null;
    }
}
