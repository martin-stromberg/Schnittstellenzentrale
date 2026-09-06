namespace Schnittstellenzentrale.Core.Models;

/// <summary>Verweis auf einen Link innerhalb einer Anwendung.</summary>
public class ApplicationLink
{
    /// <summary>Der eindeutige Bezeichner.</summary>
    public int Id { get; set; }

    /// <summary>ID der zugeordneten Anwendung.</summary>
    public int ApplicationId { get; set; }

    /// <summary>Die zugeordnete Anwendung.</summary>
    public Application Application { get; set; } = null!;

    /// <summary>Die URL des Links (optional).</summary>
    public string? Url { get; set; }

    /// <summary>Die Bezeichnung des Links (optional).</summary>
    public string? Label { get; set; }

    /// <summary>Die Sortierreihenfolge (optional).</summary>
    public int? SortOrder { get; set; }

    /// <summary>Die Zeilenversionskennung für Optimistic Concurrency.</summary>
    public byte[] RowVersion { get; set; } = [];
}
