using Microsoft.JSInterop;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;

namespace Schnittstellenzentrale.Infrastructure.Services;

/// <summary>Implementiert <see cref="IThemeService"/> mit <c>localStorage</c>-Persistierung über ein JS-Modul.</summary>
public class ThemeService : IThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;

    /// <inheritdoc/>
    public ColorScheme CurrentScheme { get; private set; } = ColorScheme.Light;

    /// <inheritdoc/>
    public event Action? OnThemeChanged;

    /// <summary>Initialisiert eine neue Instanz von <see cref="ThemeService"/>.</summary>
    /// <param name="jsRuntime">JS-Runtime für den Zugriff auf das Theme-Modul.</param>
    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <inheritdoc/>
    public async Task SetTheme(ColorScheme scheme)
    {
        if (!Enum.IsDefined(typeof(ColorScheme), scheme))
            throw new ArgumentOutOfRangeException(nameof(scheme));
        if (CurrentScheme == scheme)
            return;
        CurrentScheme = scheme;
        await PersistTheme(scheme);
        OnThemeChanged?.Invoke();
    }

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        var module = await GetModuleAsync();
        if (module == null)
            return;
        try
        {
            var stored = await module.InvokeAsync<string?>("getStoredTheme");
            if (stored != null && Enum.TryParse<ColorScheme>(stored, ignoreCase: true, out var parsed))
                CurrentScheme = parsed;
        }
        catch (Exception ex) when (ex is JSException or TaskCanceledException)
        {
        }
    }

    private async Task PersistTheme(ColorScheme scheme)
    {
        try
        {
            var module = await GetModuleAsync();
            if (module == null)
                return;
            await module.InvokeVoidAsync("setStoredTheme", scheme.ToString());
            await module.InvokeVoidAsync("applyTheme", scheme.ToString());
        }
        catch (Exception ex) when (ex is JSException or TaskCanceledException)
        {
        }
    }

    private async Task<IJSObjectReference?> GetModuleAsync()
    {
        try
        {
            _module ??= await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./theme.js");
        }
        catch (Exception ex) when (ex is JSException or TaskCanceledException)
        {
        }
        return _module;
    }
}
