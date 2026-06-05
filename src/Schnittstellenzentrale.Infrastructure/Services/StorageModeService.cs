using Microsoft.JSInterop;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;

namespace Schnittstellenzentrale.Infrastructure.Services;

/// <summary>Implementiert <see cref="IStorageModeService"/> mit <c>localStorage</c>-Persistierung über ein JS-Modul.</summary>
public class StorageModeService : IStorageModeService
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;

    /// <inheritdoc/>
    public StorageMode CurrentMode { get; private set; } = StorageMode.Team;

    /// <inheritdoc/>
    public event Action? OnModeChanged;

    /// <summary>Initialisiert eine neue Instanz von <see cref="StorageModeService"/>.</summary>
    public StorageModeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <inheritdoc/>
    public void SetMode(StorageMode mode)
    {
        if (CurrentMode == mode)
            return;
        CurrentMode = mode;
        _ = PersistModeAsync(mode);
        OnModeChanged?.Invoke();
    }

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        var module = await GetModuleAsync();
        var stored = await module.InvokeAsync<string?>("getStoredMode");
        if (stored != null && Enum.TryParse<StorageMode>(stored, ignoreCase: true, out var parsed))
        {
            CurrentMode = parsed;
            OnModeChanged?.Invoke();
        }
    }

    private async Task PersistModeAsync(StorageMode mode)
    {
        try
        {
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("setStoredMode", mode.ToString());
        }
        catch (Exception ex) when (ex is JSException or TaskCanceledException)
        {
        }
    }

    private async Task<IJSObjectReference> GetModuleAsync()
    {
        _module ??= await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./storage-mode.js");
        return _module;
    }
}
