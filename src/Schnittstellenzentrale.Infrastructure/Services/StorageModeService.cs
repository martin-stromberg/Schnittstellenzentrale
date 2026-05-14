using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;

namespace Schnittstellenzentrale.Infrastructure.Services;

public class StorageModeService : IStorageModeService
{
    public StorageMode CurrentMode { get; private set; } = StorageMode.Team;
    public event Action? OnModeChanged;

    public void SetMode(StorageMode mode)
    {
        if (CurrentMode == mode)
            return;
        CurrentMode = mode;
        OnModeChanged?.Invoke();
    }
}
