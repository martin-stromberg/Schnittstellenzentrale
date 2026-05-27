using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Helpers;

namespace Schnittstellenzentrale.Tests.Helpers;

/// <summary>Tests für <see cref="LocalStorageKeys"/>.</summary>
public class LocalStorageKeysTests
{
    /// <summary>Schlüssel für Team-Modus entspricht dem erwarteten Format.</summary>
    [Fact]
    public void SelectedEnvironmentId_TeamModus_GibtKorrektesFormat()
    {
        var key = LocalStorageKeys.SelectedEnvironmentId(StorageMode.Team);

        Assert.Equal("selectedEnvironmentId_Team", key);
    }

    /// <summary>Schlüssel für User-Modus entspricht dem erwarteten Format.</summary>
    [Fact]
    public void SelectedEnvironmentId_UserModus_GibtKorrektesFormat()
    {
        var key = LocalStorageKeys.SelectedEnvironmentId(StorageMode.User);

        Assert.Equal("selectedEnvironmentId_User", key);
    }
}
