# Bestandsaufnahme: Impressum-Seite (Issue #40)

Analysiert wurden alle für das Impressum-Feature relevanten Artefakte im `src/`-Verzeichnis: UI-Komponenten, Lokalisierungsressourcen, Konfiguration, Service-Infrastruktur und Tests. Grundlage ist die Anforderungsbeschreibung in `requirement.md`.

## Zusammenfassung

- **`WorkspacesSidebar`** enthält bereits einen statisch verdrahteten Link auf `/impressum` im Footer (`<a href="/impressum" ...>`). Der Link wird bedingungslos gerendert — eine Steuerung über `IImpressumService.IsAvailable()` fehlt noch.
- Die Lokalisierungsschlüssel `WorkspacesSidebar_ImpressumLink` sind in beiden resx-Dateien vorhanden (`Imprint` / `Impressum`). Die Schlüssel `ImpressumPage_PageTitle` und `ImpressumPage_Heading` fehlen noch.
- **`ImpressumPage`**, **`IImpressumService`**, **`ImpressumService`** und **`ImpressumSettings`** existieren noch nicht.
- **Markdig** ist in keinem der Projekte als NuGet-Paket vorhanden und muss ergänzt werden.
- Das Konfigurationsmuster (`Configure<T>`, Settings-Klasse, `appsettings.json`-Abschnitt) ist durch `HistorySettings` / `UploadSettings` etabliert und kann direkt übernommen werden.
- Die Service-Registrierung in `Program.cs` ist für `IImpressumService` noch nicht eingetragen.
- Die Playwright-Testinfrastruktur ist vollständig vorhanden (`PlaywrightServer`, `PlaywrightTestBase`, `[Collection("Playwright")]`). Impressum-spezifische Tests fehlen noch.
- `HelpPage` ist ein direktes Strukturvorbild für `ImpressumPage` (gleiches `@page`/Localizer-Muster).

## Details

- [UI-Komponenten](inventory/ui.md)
- [Logik](inventory/logic.md)
- [Interfaces](inventory/interfaces.md)
- [Lokalisierung](inventory/localization.md)
- [Konfiguration](inventory/configuration.md)
- [Abhängigkeiten / NuGet-Pakete](inventory/dependencies.md)
- [Tests](inventory/tests.md)
