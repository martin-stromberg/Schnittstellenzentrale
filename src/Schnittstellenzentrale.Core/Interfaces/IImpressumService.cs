namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>Abstraktion für Dateiverfügbarkeitsprüfung und Markdown-zu-HTML-Konvertierung der Impressum-Seite.</summary>
public interface IImpressumService
{
    /// <summary>Gibt <c>true</c> zurück, wenn die konfigurierte Impressum-Datei existiert.</summary>
    bool IsAvailable();

    /// <summary>Liest die Impressum-Datei und gibt den Inhalt als HTML-String zurück.</summary>
    Task<string> GetContentAsHtmlAsync();
}
