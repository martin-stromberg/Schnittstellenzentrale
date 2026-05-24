using Schnittstellenzentrale.Core.Enums;

namespace Schnittstellenzentrale.Core.Helpers;

public static class LocalStorageKeys
{
    public static string SelectedEnvironmentId(StorageMode mode) => $"selectedEnvironmentId_{mode}";
}
