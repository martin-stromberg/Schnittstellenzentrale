# Bestandsaufnahme: Mehrsprachige Impressum-Dateien

Analysiert wurden alle Klassen und Komponenten, die an der Anforderung zur sprachabhängigen Impressum-Dateiauflösung beteiligt sind: `IImpressumService`, `ImpressumService`, `ImpressumSettings`, `ImpressumPage`, `WorkspacesSidebar` sowie die zugehörigen Unit- und Playwright-Tests.

## Zusammenfassung

- `IImpressumService` definiert zwei parameterlose Methoden (`IsAvailable()`, `GetContentAsHtmlAsync()`). Ein Sprachparameter existiert nicht.
- `ImpressumService` löst den Dateipfad **einmalig im Konstruktor** auf (`_resolvedPath`). Eine Laufzeit-Sprachauflösung ist nicht implementiert.
- `ImpressumService` ist als **`Singleton`** registriert — im aktuellen Zustand wäre eine interne Nutzung von `CultureInfo.CurrentUICulture` nicht zuverlässig möglich.
- `ImpressumSettings` hat genau eine Eigenschaft (`FilePath`). Keine sprachspezifische Konfiguration vorhanden.
- `ImpressumPage` und `WorkspacesSidebar` rufen `ImpressumService` ohne Sprachkontext auf. Weder übergeben sie ein Sprachkürzel noch lesen sie `CultureInfo.CurrentUICulture` aus.
- Unit-Tests decken Pfadauflösung und grundlegendes `IsAvailable`-/`GetContent`-Verhalten ab; Testfälle für sprachspezifische Dateiauflösung und Fallback fehlen vollständig.
- Playwright-Tests prüfen Datei-vorhanden / Datei-fehlt-Szenarios; `Accept-Language`-basierte Tests fehlen.

## Details

- [Interfaces](inventory/interfaces.md)
- [Logik](inventory/logic.md)
- [UI-Komponenten](inventory/ui.md)
- [Tests](inventory/tests.md)
