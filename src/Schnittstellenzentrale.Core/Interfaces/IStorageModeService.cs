using Schnittstellenzentrale.Core.Enums;

namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>Definiert Operationen für den Speichermodus.</summary>
public interface IStorageModeService
{
    /// <summary>Ruft den aktuellen Speichermodus ab.</summary>
    StorageMode CurrentMode { get; }

    /// <summary>Tritt ein, wenn sich der Speichermodus ändert.</summary>
    event Action? OnModeChanged;

    /// <summary>Legt den Speichermodus fest.</summary>
    /// <param name="mode">Der zu verwendende Speichermodus.</param>
    void SetMode(StorageMode mode);

    /// <summary>Initialisiert den Dienst asynchron.</summary>
    Task InitializeAsync();
}
