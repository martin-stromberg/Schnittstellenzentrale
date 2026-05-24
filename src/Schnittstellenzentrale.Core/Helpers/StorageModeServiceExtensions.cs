using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;

namespace Schnittstellenzentrale.Core.Helpers;

public static class StorageModeServiceExtensions
{
    public static string? GetCurrentOwner(this IStorageModeService storageModeService, ICurrentUserService currentUserService)
    {
        return storageModeService.CurrentMode == StorageMode.User
            ? currentUserService.GetCurrentUserName()
            : null;
    }
}
