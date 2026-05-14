using Schnittstellenzentrale.Core.Enums;

namespace Schnittstellenzentrale.Core.Interfaces;

public interface IStorageModeService
{
    StorageMode CurrentMode { get; }
    event Action? OnModeChanged;
    void SetMode(StorageMode mode);
}
