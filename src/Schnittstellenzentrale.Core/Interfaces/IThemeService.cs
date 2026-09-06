using Schnittstellenzentrale.Core.Enums;

namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>Definiert Operationen für das Farbschema.</summary>
public interface IThemeService
{
    /// <summary>Ruft das aktuelle Farbschema ab.</summary>
    ColorScheme CurrentScheme { get; }

    /// <summary>Tritt ein, wenn sich das Farbschema ändert.</summary>
    event Action? OnThemeChanged;

    /// <summary>Initialisiert den Dienst asynchron.</summary>
    Task InitializeAsync();

    /// <summary>Legt das Farbschema fest.</summary>
    /// <param name="scheme">Das zu verwendende Farbschema.</param>
    Task SetTheme(ColorScheme scheme);
}
