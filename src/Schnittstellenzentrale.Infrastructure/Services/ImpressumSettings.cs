namespace Schnittstellenzentrale.Infrastructure.Services;

/// <summary>Konfigurationsklasse für Impressum-Einstellungen.</summary>
public class ImpressumSettings
{
    /// <summary>Pfad zur Impressum-Markdown-Datei. Leer bedeutet AppContext.BaseDirectory/impressum.md.</summary>
    public string FilePath { get; set; } = string.Empty;
}
