using Microsoft.Extensions.Logging;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Infrastructure.Services;

/// <summary>Implementierung von <see cref="INavigationStateService"/>.</summary>
public class NavigationStateService : INavigationStateService
{
    private readonly ILogger<NavigationStateService> _logger;

    /// <summary>Initialisiert eine neue Instanz von <see cref="NavigationStateService"/>.</summary>
    public NavigationStateService(ILogger<NavigationStateService> logger)
    {
        _logger = logger;
    }

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
        try
        {
            OnAreaChanged?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Fehler im OnAreaChanged-Event-Handler.");
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task SetWorkspaceSelectionAsync(WorkspaceSelection? selection)
    {
        CurrentSelection = selection;
        try
        {
            OnSelectionChanged?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Fehler im OnSelectionChanged-Event-Handler.");
        }
        return Task.CompletedTask;
    }
}
