#pragma warning disable CS1591
using Microsoft.JSInterop;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;

namespace Schnittstellenzentrale.Infrastructure.Services;

public class ThemeService : IThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;

    public ColorScheme CurrentScheme { get; private set; } = ColorScheme.Light;
    public event Action? OnThemeChanged;

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

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

    public async Task InitializeAsync()
    {
        var module = await GetModuleAsync();
        var stored = await module.InvokeAsync<string?>("getStoredTheme");
        if (stored != null && Enum.TryParse<ColorScheme>(stored, ignoreCase: true, out var parsed))
            CurrentScheme = parsed;
    }

    private async Task PersistTheme(ColorScheme scheme)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("setStoredTheme", scheme.ToString());
        await module.InvokeVoidAsync("applyTheme", scheme.ToString());
    }

    private async Task<IJSObjectReference> GetModuleAsync()
    {
        _module ??= await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./theme.js");
        return _module;
    }
}
