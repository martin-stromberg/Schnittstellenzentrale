namespace Schnittstellenzentrale.Core.Models;

/// <summary>Die aktuelle Auswahl im Workspace.</summary>
/// <param name="SelectedItem">Das ausgewählte Element.</param>
/// <param name="SelectionPath">Der Pfad der ausgewählten Elemente.</param>
/// <returns>Die erstellte Auswahl.</returns>
public record WorkspaceSelection(
    object SelectedItem,
    IReadOnlyList<object> SelectionPath
);
