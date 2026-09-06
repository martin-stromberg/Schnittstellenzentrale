namespace Schnittstellenzentrale.Core.Contracts;

/// <summary>
/// DTO für eine Systemumgebung.
/// </summary>
public class SystemEnvironmentResponse
{
    /// <summary>
    /// Eindeutige ID der Umgebung.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name der Umgebung.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Modus der Umgebung.
    /// </summary>
    public int Mode { get; set; }

    /// <summary>
    /// Optionaler Besitzer der Umgebung.
    /// </summary>
    public string? Owner { get; set; }

    /// <summary>
    /// Optionale Beschreibung der Umgebung.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Umgebungsvariablen dieser Umgebung.
    /// </summary>
    public IList<EnvironmentVariableResponse> Variables { get; set; } = [];
}
