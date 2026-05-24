using Schnittstellenzentrale.Core.Enums;

namespace Schnittstellenzentrale.Core.Interfaces;

public static class StorageModeServiceExtensions
{
    public static string? GetCurrentOwner(this IStorageModeService storageModeService, ICurrentUserService currentUserService)
    {
        return storageModeService.CurrentMode == StorageMode.User
            ? currentUserService.GetCurrentUserName()
            : null;
    }
}
