namespace Schnittstellenzentrale.Core.Models;

/// <summary>Umgebungsvariable innerhalb einer Systemumgebung.</summary>
public class EnvironmentVariable
{
    /// <summary>Der eindeutige Bezeichner.</summary>
    public int Id { get; set; }

    /// <summary>Der Name der Variablen.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Der Wert der Variablen.</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>Gibt an, ob der Wert maskiert angezeigt wird.</summary>
    public bool IsValueMasked { get; set; }

    /// <summary>ID der zugeordneten Systemumgebung.</summary>
    public int SystemEnvironmentId { get; set; }

    /// <summary>Die zugeordnete Systemumgebung (optional).</summary>
    public SystemEnvironment? SystemEnvironment { get; set; }
}
