# Interfaces – Bestandsaufnahme

## Noch nicht vorhanden

`IImpressumService` (geplant in `src/Schnittstellenzentrale.Core/Interfaces/`) existiert noch nicht im Codebase.

---

## Vorhandene Interfaces (Referenz für Konventionen)

Die folgenden Interfaces zeigen das etablierte Muster im Projekt und sind relevant als strukturelle Referenz für `IImpressumService`:

**`IThemeService`** — einfaches, zustandsbehaftetes Interface ohne Datenbankbezug.  
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IThemeService.cs`

**`ICurrentUserService`** — reines Lese-Interface ohne Datenbankzugriff, als Singleton registriert.  
Datei: `src/Schnittstellenzentrale.Core/Interfaces/ICurrentUserService.cs`

Alle Interfaces liegen im Namespace-Verzeichnis `src/Schnittstellenzentrale.Core/Interfaces/`.
