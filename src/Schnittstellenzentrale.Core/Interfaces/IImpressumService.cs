namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>Abstraktion für Dateiverfügbarkeitsprüfung und Markdown-zu-HTML-Konvertierung der Impressum-Seite.</summary>
public interface IImpressumService
{
    /// <summary>Gibt <c>true</c> zurück, wenn die konfigurierte Impressum-Datei existiert.</summary>
    /// <returns><c>true</c>, wenn die Datei existiert; sonst <c>false</c>.</returns>
    bool IsAvailable();

    /// <summary>Liest die Impressum-Datei und gibt den Inhalt als HTML-String zurück.</summary>
    /// <returns>HTML-Inhalt der Impressum-Seite.</returns>
    Task<string> GetContentAsHtmlAsync();
}
