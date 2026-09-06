namespace Schnittstellenzentrale.Core.Contracts;

/// <summary>
/// DTO für eine Umgebungsvariable.
/// </summary>
public class EnvironmentVariableResponse
{
    /// <summary>
    /// Eindeutige ID der Variable.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name der Variable.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Wert der Variable.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gibt an, ob der Wert maskiert dargestellt wird.
    /// </summary>
    public bool IsValueMasked { get; set; }
}
