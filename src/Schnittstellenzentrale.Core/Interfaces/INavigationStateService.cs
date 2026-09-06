using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>Definiert Operationen für den Navigationszustand.</summary>
public interface INavigationStateService
{
    /// <summary>Ruft den aktuellen Navigationsbereich ab.</summary>
    NavigationArea CurrentArea { get; }

    /// <summary>Ruft die aktuelle Arbeitsbereichsauswahl ab.</summary>
    WorkspaceSelection? CurrentSelection { get; }

    /// <summary>Ruft den aktuellen Auswahlpfad ab.</summary>
    IReadOnlyList<object> CurrentSelectionPath { get; }

    /// <summary>Tritt ein, wenn sich der Navigationsbereich ändert.</summary>
    event Action? OnAreaChanged;

    /// <summary>Tritt ein, wenn sich die Auswahl ändert.</summary>
    event Action? OnSelectionChanged;

    /// <summary>Legt den aktuellen Navigationsbereich fest.</summary>
    /// <param name="area">Der neue Navigationsbereich.</param>
    Task SetAreaAsync(NavigationArea area);

    /// <summary>Legt die aktuelle Arbeitsbereichsauswahl fest.</summary>
    /// <param name="selection">Die neue Auswahl.</param>
    Task SetWorkspaceSelectionAsync(WorkspaceSelection? selection);
}
