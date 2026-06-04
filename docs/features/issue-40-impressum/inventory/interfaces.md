# Interfaces

## `IImpressumService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IImpressumService.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `IsAvailable()` | — | `bool` | Prüft, ob die konfigurierte Impressum-Datei auf dem Dateisystem existiert |
| `GetContentAsHtmlAsync()` | — | `Task<string>` | Liest die Impressum-Datei und gibt den Inhalt als HTML-String zurück (Markdown-Rendering via Markdig) |

Beide Methoden haben **keinen Sprachparameter**. Die Pfadauflösung erfolgt vollständig im Konstruktor von `ImpressumService` — eine Laufzeit-Sprachauflösung ist im Interface noch nicht vorgesehen.
