namespace Schnittstellenzentrale.Core.Models;

public record WorkspaceSelection(
    object SelectedItem,
    IReadOnlyList<object> SelectionPath
);
