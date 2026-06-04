# Tests – Bestandsaufnahme

## Testklassen (Bestand)

Kein impressum-spezifischer Testcode vorhanden. Die folgenden Testklassen sind als Strukturreferenz relevant:

### `HomePageTests`
Datei: `src/Schnittstellenzentrale.Tests/Playwright/HomePageTests.cs`

- `StartPage_ShowsSystemGroup` — prüft via Playwright, ob die Systemgruppe im Baum sichtbar ist
- `StartPage_ShowsOwnApiEndpoints` — prüft, ob Endpunkte nach Aufklappen erscheinen

Zeigt das Muster für Playwright-Tests: `[Collection("Playwright")]`, Ableitung von `PlaywrightTestBase`, `await Page.GotoAsync(BaseUrl)`.

### `LayoutSmokeTests`
Datei: `src/Schnittstellenzentrale.Tests/Playwright/LayoutSmokeTests.cs`

- `AppShell_NimmtVollbildEin` — prüft via `BoundingBoxAsync()`, dass das Layout nicht kollabiert
- `TopBar_IstSichtbarUndHatHoehe` — Layout-Smoke-Test für die TopBar
- `WorkspacesBereich_SidebarUndContentHabenFlaeche` — Layout-Smoke-Test für Workspaces
- `EnvironmentsBereich_SidebarHatFlaeche` — Layout-Smoke-Test für Environments
- `HistoryBereich_ContentHatFlaeche` — Layout-Smoke-Test für den History-Bereich

Zeigt das etablierte Muster für Smoke-Tests mit `BoundingBoxAsync()`.

## Playwright-Infrastruktur

Verzeichnis: `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/`

| Datei | Beschreibung |
|-------|-------------|
| `PlaywrightServer.cs` | Startet Kestrel auf Port 5099; kein manueller App-Start nötig |
| `PlaywrightTestBase.cs` | Basisklasse für Playwright-Tests; stellt `Page` und `BaseUrl` bereit |
| `PlaywrightApiFactory.cs` | `WebApplicationFactory`-Ableitung für Playwright-Tests |
| `PlaywrightCollection.cs` | `[CollectionDefinition("Playwright")]`-Marker |
| `TestDatabaseSeeder.cs` | Befüllt die Testdatenbank mit Seed-Daten |

## Noch nicht vorhanden

- `ImpressumServiceTests` (Unit-Test: Datei vorhanden / nicht vorhanden, Markdown-Ausgabe)
- `ImpressumPageTests` (Playwright: Seite sichtbar wenn Datei vorhanden, Menüeintrag fehlt wenn nicht vorhanden)
