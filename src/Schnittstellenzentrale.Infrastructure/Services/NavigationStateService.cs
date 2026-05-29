using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Infrastructure.Services;

/// <summary>Implementierung von <see cref="INavigationStateService"/>.</summary>
public class NavigationStateService : INavigationStateService
{
    /// <inheritdoc/>
    public NavigationArea CurrentArea { get; private set; } = NavigationArea.Workspaces;

    /// <inheritdoc/>
    public WorkspaceSelection? CurrentSelection { get; private set; }

    /// <inheritdoc/>
    public IReadOnlyList<object> CurrentSelectionPath => CurrentSelection?.SelectionPath ?? [];

    /// <inheritdoc/>
    public event Action? OnAreaChanged;

    /// <inheritdoc/>
    public event Action? OnSelectionChanged;

    /// <inheritdoc/>
    public Task SetAreaAsync(NavigationArea area)
    {
        CurrentArea = area;
        OnAreaChanged?.Invoke();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task SetWorkspaceSelectionAsync(WorkspaceSelection? selection)
    {
        CurrentSelection = selection;
        OnSelectionChanged?.Invoke();
        return Task.CompletedTask;
    }
}
