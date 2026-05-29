using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

public interface INavigationStateService
{
    NavigationArea CurrentArea { get; }
    WorkspaceSelection? CurrentSelection { get; }
    IReadOnlyList<object> CurrentSelectionPath { get; }
    event Action? OnAreaChanged;
    event Action? OnSelectionChanged;
    Task SetAreaAsync(NavigationArea area);
    Task SetWorkspaceSelectionAsync(WorkspaceSelection? selection);
}
