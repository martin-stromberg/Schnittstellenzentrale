# Umsetzungsplan: Mehrsprachige Impressum-Dateien

## Übersicht

`ImpressumService` wird um eine Laufzeit-Sprachauflösung erweitert: Statt eines einmalig im Konstruktor festgelegten Pfades ermittelt der Service bei jedem Aufruf anhand von `CultureInfo.CurrentUICulture` die passende Markdown-Datei (`impressum.<language>.md` mit Fallback auf `impressum.md`). Die Registrierung des Services wird von `Singleton` auf `Scoped` umgestellt, damit die Kultur pro Request korrekt gelesen wird. Das Interface `IImpressumService` bleibt unverändert.

## Designentscheidungen

| Komponente / Bereich | Gewählter Ansatz | Begründung |
|----------------------|-----------------|------------|
| Kulturzugriff in `ImpressumService` | Service liest `CultureInfo.CurrentUICulture.TwoLetterISOLanguageName` intern in `IsAvailable()` und `GetContentAsHtmlAsync()` — kein Sprachparameter im Interface | Hält `IImpressumService` stabil, vermeidet Parameterweiterleitung durch alle Aufrufer; im Scoped-Lebenszyklus ist `CultureInfo.CurrentUICulture` pro Request zuverlässig gesetzt (RequestLocalizationMiddleware). Option A (expliziter Parameter) wäre besser testbar, erfordert aber Interface-Änderung und Anpassung aller Aufrufer — Mehraufwand ohne fachlichen Mehrwert. |
| Service-Lebenszyklus | Umstellung von `Singleton` auf `Scoped` | Einzige Voraussetzung für zuverlässige interne Kulturlesbarkeit; keine Nebenwirkungen, da `ImpressumService` keinen gemeinsamen Zustand zwischen Requests hält. |
| Pfadauflösung | Laufzeit-Methode `ResolvePath(string language)` intern in `ImpressumService` — Basisverzeichnis und Basisname werden weiterhin im Konstruktor einmalig aus `_resolvedPath` abgeleitet | Konstruktor-Logik bleibt unverändert; nur die Verwendung von `_resolvedPath` in `IsAvailable()` und `GetContentAsHtmlAsync()` wird durch einen Aufruf der neuen Methode ersetzt. |
| Sprachkürzel-Format | `CultureInfo.TwoLetterISOLanguageName` (z. B. `"de"`, `"en"`) | Anforderung zeigt `impressum.de.md`; zweistellig ist die minimalste und am weitesten verbreitete Konvention für Dateinamen. |
| Sidebar-Verhalten | Footer-Link erscheint, wenn `IsAvailable()` mit der aktuellen Sprache (inkl. Fallback auf `impressum.md`) `true` zurückgibt | Die Anforderung impliziert den Fallback: solange irgendeine Impressum-Datei (sprachspezifisch oder Fallback) vorhanden ist, soll der Link erscheinen. |

## Programmabläufe

### Sprachabhängige Impressum-Verfügbarkeitsprüfung (`IsAvailable`)

1. `WorkspacesSidebar.OnInitialized()` ruft `IImpressumService.IsAvailable()` auf.
2. `ImpressumService.IsAvailable()` liest `CultureInfo.CurrentUICulture.TwoLetterISOLanguageName` aus.
3. `ImpressumService.ResolvePath(language)` prüft, ob `<baseDir>/<baseName>.<language>.md` existiert; falls ja, wird dieser Pfad zurückgegeben, sonst `_resolvedPath` (Fallback).
4. `File.Exists(resolvedPath)` liefert das Ergebnis zurück.
5. `WorkspacesSidebar` setzt `_impressumAvailable` und rendert den Footer-Link entsprechend.

Beteiligte Klassen/Komponenten: `WorkspacesSidebar`, `IImpressumService`, `ImpressumService`

---

### Sprachabhängige Impressum-Inhalt-Anzeige (`GetContentAsHtmlAsync`)

1. `ImpressumPage.OnInitializedAsync()` ruft `IImpressumService.IsAvailable()` auf (Ablauf wie oben).
2. Falls verfügbar, ruft `ImpressumPage` `IImpressumService.GetContentAsHtmlAsync()` auf.
3. `ImpressumService.GetContentAsHtmlAsync()` liest `CultureInfo.CurrentUICulture.TwoLetterISOLanguageName` aus.
4. `ImpressumService.ResolvePath(language)` ermittelt den Pfad (sprachspezifisch oder Fallback).
5. `File.ReadAllTextAsync(resolvedPath)` liest die Datei.
6. `Markdig.Markdown.ToHtml(content)` wandelt Markdown in HTML um.
7. `ImpressumPage` setzt `_htmlContent` und rendert den Inhalt.

Beteiligte Klassen/Komponenten: `ImpressumPage`, `IImpressumService`, `ImpressumService`

---

### Laufzeit-Pfadauflösung (`ResolvePath`)

1. `baseDir` und `baseName` werden aus dem im Konstruktor aufgelösten `_resolvedPath` abgeleitet (`Path.GetDirectoryName`, `Path.GetFileNameWithoutExtension`).
2. Ist `language` nicht leer: Kandidatenpfad `Path.Combine(baseDir, $"{baseName}.{language}.md")` bilden.
3. `File.Exists(candidate)` — bei `true`: Kandidatenpfad zurückgeben.
4. Sonst: `_resolvedPath` zurückgeben.

Beteiligte Klassen/Komponenten: `ImpressumService`

## Neue Klassen

Keine.

## Änderungen an bestehenden Klassen

### `ImpressumService` (Klasse)

- **Neue Felder:** `_baseDir` (`string`) — Verzeichnis der Fallback-Datei, im Konstruktor aus `_resolvedPath` abgeleitet; `_baseName` (`string`) — Dateiname ohne Erweiterung, ebenfalls aus `_resolvedPath` abgeleitet
- **Neue Methoden:** `ResolvePath(string language)` — ermittelt zur Laufzeit den zu verwendenden Dateipfad; gibt Kandidatenpfad zurück wenn Datei existiert, sonst `_resolvedPath`; `private string`
- **Geänderte Methoden:**
  - `IsAvailable()` — ruft jetzt intern `CultureInfo.CurrentUICulture.TwoLetterISOLanguageName` ab und übergibt es an `ResolvePath`; ersetzt `File.Exists(_resolvedPath)` durch `File.Exists(ResolvePath(language))`
  - `GetContentAsHtmlAsync()` — ruft jetzt intern `CultureInfo.CurrentUICulture.TwoLetterISOLanguageName` ab und übergibt es an `ResolvePath`; ersetzt `_resolvedPath` durch `ResolvePath(language)` beim Dateilesen

### `Program.cs` (Konfigurationsdatei / DI-Registrierung)

- **Geänderte Registrierung:** `AddSingleton<IImpressumService, ImpressumService>()` → `AddScoped<IImpressumService, ImpressumService>()` — notwendig damit `CultureInfo.CurrentUICulture` pro Request korrekt gelesen wird

## Datenbankmigrationen

Keine.

## Validierungsregeln

Keine.

## Konfigurationsänderungen

Keine.

## Seiteneffekte und Risiken

- **Lebenszyklus-Umstellung auf Scoped:** `ImpressumService` wird nun einmal pro Request instanziiert statt einmal pro App-Start. Da der Service keinen shared State zwischen Requests hält, entstehen keine Datenkonsistenzprobleme. Leicht erhöhter Instanziierungsaufwand ist vernachlässigbar.
- **Singleton-Abhängigkeiten:** Falls andere Singleton-Services `IImpressumService` injizieren, würde die Scoped-Registrierung zur Laufzeit eine Captive-Dependency-Exception auslösen. Laut Bestandsaufnahme wird `IImpressumService` ausschließlich in den Blazor-Komponenten `ImpressumPage` und `WorkspacesSidebar` verwendet — keine Singleton-Abhängigkeit bekannt. Beim Umstellen dennoch prüfen.
- **`IImpressumPage` und `WorkspacesSidebar`:** Keine Code-Änderungen erforderlich; die Kultur wird intern im Service gelesen. Die Komponenten verhalten sich für den Aufrufer transparent weiter.

## Umsetzungsreihenfolge

1. `ImpressumService`: `_baseDir`- und `_baseName`-Felder im Konstruktor aus `_resolvedPath` ableiten
2. `ImpressumService`: private Methode `ResolvePath(string language)` implementieren
3. `ImpressumService`: `IsAvailable()` anpassen — Kultur lesen, `ResolvePath` aufrufen
4. `ImpressumService`: `GetContentAsHtmlAsync()` anpassen — Kultur lesen, `ResolvePath` aufrufen
5. `Program.cs`: Registrierung von `Singleton` auf `Scoped` umstellen
6. `ImpressumServiceTests`: Hilfsmethode `CreateService` um Überladung ergänzen, die Testdateien im temporären Verzeichnis anlegt; neue Unit-Testfälle für Sprachauflösung hinzufügen
7. Playwright-Infrastruktur: `PlaywrightImpressumServer` um Variante mit sprachspezifischer Datei (`impressum.de.md`) erweitern oder neue Server-Fixture anlegen
8. `ImpressumPageTests`: neue Playwright-Testfälle für `Accept-Language`-basiertes Verhalten hinzufügen

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `IsAvailable_SprachspezifischeDateiVorhanden_GibtTrueZurueck` | `ImpressumServiceTests` | Sprachspezifische Datei (`impressum.de.md`) vorhanden → `IsAvailable()` mit Kultur `"de"` gibt `true` zurück |
| `IsAvailable_SprachspezifischeDateiFehlt_FallbackVorhanden_GibtTrueZurueck` | `ImpressumServiceTests` | Sprachspezifische Datei fehlt, Fallback (`impressum.md`) vorhanden → `IsAvailable()` gibt `true` zurück |
| `IsAvailable_BeideVariantenFehlen_GibtFalseZurueck` | `ImpressumServiceTests` | Weder sprachspezifische Datei noch Fallback vorhanden → `IsAvailable()` gibt `false` zurück |
| `IsAvailable_NeutralesSprachkuerzel_VerwendetFallback` | `ImpressumServiceTests` | Leere oder `null`-Sprache → direkt Fallback-Pfad wird geprüft |
| `GetContentAsHtmlAsync_SprachspezifischeDateiVorhanden_LiestSprachspezifischeDatei` | `ImpressumServiceTests` | Sprachspezifische Datei vorhanden → Inhalt der sprachspezifischen Datei wird zurückgegeben, nicht der Fallback |
| `GetContentAsHtmlAsync_SprachspezifischeDateiFehlt_LiestFallbackDatei` | `ImpressumServiceTests` | Sprachspezifische Datei fehlt, Fallback vorhanden → Inhalt des Fallbacks wird zurückgegeben |
| `ResolvePath_SprachspezifischeDateiVorhanden_GibtSprachspezifischenPfadZurueck` | `ImpressumServiceTests` | Interne Pfadauflösung: sprachspezifische Datei vorhanden → Kandidatenpfad wird zurückgegeben |
| `ResolvePath_SprachspezifischeDateiFehlt_GibtFallbackPfadZurueck` | `ImpressumServiceTests` | Interne Pfadauflösung: Datei fehlt → `_resolvedPath` wird zurückgegeben |
| `ImpressumSeite_ZeigtSprachspezifischenInhalt_BeiDeutscherSprache` | `ImpressumPageWithLanguageTests` (neu) | Playwright: `Accept-Language: de` → Seite zeigt Inhalt aus `impressum.de.md` |
| `ImpressumSeite_ZeigtFallbackInhalt_WennSprachspezifischeDateiFehlt` | `ImpressumPageWithLanguageTests` (neu) | Playwright: `Accept-Language: fr` + nur `impressum.md` vorhanden → Seite zeigt Fallback-Inhalt |
| `PlaywrightImpressumLanguageServer` (Fixture) | Playwright-Infrastruktur | Neue Server-Fixture, die neben `impressum.md` auch `impressum.de.md` mit eigenem Inhalt anlegt; Port 5102 |
| `PlaywrightImpressumLanguageCollection` (Collection) | Playwright-Infrastruktur | xUnit-Collection-Definition für `PlaywrightImpressumLanguageServer` |

### Betroffene bestehende Tests

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| `IsAvailable_DateiVorhanden_GibtTrueZurueck` | Kultur muss in der Testumgebung auf einen bekannten Wert gesetzt werden, damit die neue `ResolvePath`-Logik den erwarteten Pfad prüft (keine sprachspezifische Datei vorhanden → Fallback) |
| `GetContentAsHtmlAsync_MarkdownWirdKorrektGerendert` | Analog: Kultur muss auf Wert ohne passende sprachspezifische Datei gesetzt werden, damit Fallback-Pfad verwendet wird |
| `GetContentAsHtmlAsync_DateiFehlt_WirftException` | Analog: Sicherstellen, dass weder sprachspezifische noch Fallback-Datei existiert |

## Offene Punkte

Keine.
