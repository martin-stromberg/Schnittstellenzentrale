using Schnittstellenzentrale.Core.Enums;

namespace Schnittstellenzentrale.Core.Interfaces;

public interface IThemeService
{
    ColorScheme CurrentScheme { get; }
    event Action? OnThemeChanged;
    Task InitializeAsync();
    Task SetTheme(ColorScheme scheme);
}
