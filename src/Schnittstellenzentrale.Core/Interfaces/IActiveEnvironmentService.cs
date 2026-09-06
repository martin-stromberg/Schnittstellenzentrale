using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>Definiert den Zugriff auf die aktive Systemumgebung und deren Variablen.</summary>
public interface IActiveEnvironmentService
{
    /// <summary>Ruft die aktive Umgebung ab.</summary>
    SystemEnvironment? ActiveEnvironment { get; }

    /// <summary>Ruft die aktiven Umgebungsvariablen ab.</summary>
    IReadOnlyDictionary<string, string> ActiveVariables { get; }

    /// <summary>Tritt ein, wenn sich die aktive Umgebung ändert.</summary>
    event Action? OnActiveEnvironmentChanged;

    /// <summary>Tritt ein, wenn sich die Umgebungsliste ändert.</summary>
    event Action? OnEnvironmentListChanged;

    /// <summary>Legt die aktive Umgebung fest.</summary>
    /// <param name="environment">Die zu aktivierende Umgebung.</param>
    void SetActiveEnvironment(SystemEnvironment? environment);

    /// <summary>Benachrichtigt über eine Änderung der Umgebungsliste.</summary>
    void NotifyEnvironmentListChanged();
}
