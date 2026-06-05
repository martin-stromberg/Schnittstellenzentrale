# Übersetzte Anforderung – Mehrsprachige Impressum-Dateien

## Fachliche Zusammenfassung

`IImpressumService` wird um Sprachauflösung erweitert: Statt immer einen festen Pfad (`_resolvedPath`) zu verwenden, ermittelt der Service zur Laufzeit anhand einer übergebenen Sprachpräferenz die passende Markdown-Datei. Die Suchstrategie folgt einem Prioritätsmuster: zuerst `impressum.<sprache>.md` (z. B. `impressum.de.md`, `impressum.en.md`), dann als Fallback `impressum.md`. `ImpressumPage` und `WorkspacesSidebar` müssen die aktuelle Browser-Sprache an den Service weitergeben, da im Blazor Server-Kontext `CultureInfo.CurrentUICulture` die durch `RequestLocalizationMiddleware` gesetzte Kultur des Requests trägt.

## Betroffene Klassen und Komponenten

### Interfaces

- `IImpressumService` — Signaturerweiterung oder neue Überladungen für `IsAvailable()` und `GetContentAsHtmlAsync()`:
  - Option A: Sprachparameter in beide Methoden einführen (`IsAvailable(string? language)`, `GetContentAsHtmlAsync(string? language)`)
  - Option B: Sprachloses Interface behalten, `ImpressumService` liest die Kultur intern über `IHttpContextAccessor` (**Annahme:** Blazor Server, kein Pre-Rendering-Problem)

### Logikklassen / Services

- `ImpressumService` — Kernänderung: Der bislang einmalig im Konstruktor aufgelöste `_resolvedPath` wird durch eine Methode zur Laufzeit-Pfadauflösung ersetzt, die den Basis-Verzeichnispfad und ein optionales Sprachkürzel berücksichtigt:
  1. `impressum.<language>.md` im Basis-Verzeichnis prüfen
  2. Falls nicht vorhanden: Fallback auf `impressum.md` (bzw. den konfigurierten `FilePath`)
- `ImpressumSettings` — keine Änderung an der bestehenden `FilePath`-Eigenschaft vorgesehen; `FilePath` wird als Basis für die Namensableitung der sprachspezifischen Dateien interpretiert (**Annahme:** Betreiber legt alle Sprachvarianten im selben Verzeichnis mit gleichem Basisnamen ab)

### UI-Komponenten

- `ImpressumPage` — muss die aktuelle Sprache (`CultureInfo.CurrentUICulture.TwoLetterISOLanguageName`) an den Service übergeben, bevor `IsAvailable()` und `GetContentAsHtmlAsync()` aufgerufen werden
- `WorkspacesSidebar` — analog: `IsAvailable()` muss sprachbewusst aufgerufen werden, damit der Footer-Link nur erscheint, wenn eine passende Datei vorhanden ist

### Tests

- `ImpressumServiceTests` — bestehende Unit-Tests anpassen; neue Testfälle für:
  - Sprachspezifische Datei vorhanden → sprachspezifischer Pfad wird verwendet
  - Sprachspezifische Datei nicht vorhanden, Fallback-Datei vorhanden → Fallback-Pfad wird verwendet
  - Weder sprachspezifische noch Fallback-Datei vorhanden → `IsAvailable()` gibt `false` zurück
  - Neutrales Sprachkürzel (z. B. `""` oder `null`) → direkt Fallback-Datei
- `ImpressumPageTests` (Playwright) — Integrationstests für das Sprachauswahlverhalten über den `Accept-Language`-Header

## Implementierungsansatz

Die `RequestLocalizationMiddleware` setzt bereits `CultureInfo.CurrentUICulture` auf Basis des `Accept-Language`-Headers (siehe Lokalisierungsablauf). Im Blazor Server-Kontext ist `CultureInfo.CurrentUICulture` innerhalb von `OnInitializedAsync()` auf den Wert des initialen HTTP-Requests gesetzt.

**Empfohlener Ansatz:** `ImpressumService` liest `CultureInfo.CurrentUICulture.TwoLetterISOLanguageName` selbst zur Laufzeit in `IsAvailable()` und `GetContentAsHtmlAsync()` aus. Das hält das Interface unverändert und vermeidet Weitergabe von Sprachparametern durch alle Aufrufer. Voraussetzung: `ImpressumService` ist als `Scoped`- oder `Transient`-Service registriert (nicht `Singleton`), damit jeder Request eine frische Kultur sieht.

Pfadauflösung zur Laufzeit (Pseudologik):

```
baseDir  = Verzeichnis von _resolvedPath (Konstruktor wie bisher)
baseName = Dateiname ohne Erweiterung (z. B. "impressum")
ext      = ".md"
language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName  // z. B. "de", "en"

candidate = Path.Combine(baseDir, $"{baseName}.{language}{ext}")
if File.Exists(candidate) → candidate verwenden
else → _resolvedPath (Fallback)
```

Abhängigkeiten:
- Keine neuen NuGet-Pakete erforderlich
- `CultureInfo` ist über `System.Globalization` bereits verfügbar
- Bestehende `Markdig`-Abhängigkeit bleibt unverändert

## Konfiguration

Keine neue Konfigurationsoption erforderlich. `ImpressumSettings.FilePath` dient weiterhin als Basispfad; sprachspezifische Dateien werden durch Einsetzen des Sprachkürzels vor die Erweiterung gesucht.

**Annahme:** Betreiber legt alle Sprachvarianten im selben Verzeichnis ab und verwendet den Basisnamen der konfigurierten Datei (z. B. `impressum.de.md` neben `impressum.md`). Eine separate Konfiguration pro Sprache ist nicht vorgesehen.

## Offene Fragen

1. **Interface-Stabilität:** Soll `IImpressumService` unverändert bleiben (Service liest Kultur intern), oder ist ein expliziter Sprachparameter in `IsAvailable(string? language)` / `GetContentAsHtmlAsync(string? language)` bevorzugt (bessere Testbarkeit, expliziteres API)?

2. **Service-Lebenszyklus:** Ist `ImpressumService` aktuell als `Singleton` registriert? Falls ja, kann `CultureInfo.CurrentUICulture` nicht zuverlässig intern gelesen werden — dann ist entweder Umstellung auf `Scoped`/`Transient` oder der explizite Sprachparameter erforderlich.

3. **Normalisierung des Sprachkürzels:** Soll `CultureInfo.TwoLetterISOLanguageName` (z. B. `"de"`) verwendet werden, oder der vollständige Kulturname (z. B. `"de-DE"`)? Die Anforderung zeigt `impressum.de.md`, was auf das zweistellige ISO-Kürzel hindeutet.

4. **Verhalten bei unbekannter Sprache:** Wenn weder `impressum.fr.md` noch `impressum.md` existiert, gibt `IsAvailable()` `false` zurück — ist das gewünschte Verhalten?

5. **Sidebar-Verhalten:** Soll der Footer-Link in `WorkspacesSidebar` erscheinen, sobald *irgendeine* Impressum-Datei vorhanden ist (Fallback genügt), oder nur wenn eine sprachlich passende Datei existiert? Die Anforderung impliziert Letzteres durch den Fallback auf `impressum.md`.
