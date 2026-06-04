# Tests

## Testklassen

### `ImpressumServiceTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/ImpressumServiceTests.cs`

Unit-Tests für `ImpressumService`. Verwendet ein temporäres Verzeichnis (`_tempDir`), das im Konstruktor erstellt und in `Dispose()` gelöscht wird.

- `IsAvailable_DateiVorhanden_GibtTrueZurueck` — Datei am konfigurierten Pfad vorhanden → `IsAvailable()` gibt `true` zurück
- `IsAvailable_DateiFehlt_GibtFalseZurueck` — Datei fehlt → `IsAvailable()` gibt `false` zurück
- `GetContentAsHtmlAsync_MarkdownWirdKorrektGerendert` — Markdown-Inhalt `# Titel` wird korrekt zu `<h1>Titel</h1>` gerendert
- `GetContentAsHtmlAsync_DateiFehlt_WirftException` — fehlende Datei → `FileNotFoundException` wird geworfen
- `Pfadaufloesung_LeerFilePath_VerwendetBaseDirectory` — leerer `FilePath` → Pfad wird auf `AppContext.BaseDirectory/impressum.md` aufgelöst
- `Pfadaufloesung_RelativerFilePath_WirdRelativZuBaseDirectoryAufgeloest` — relativer Pfad → wird relativ zu `AppContext.BaseDirectory` aufgelöst
- `Pfadaufloesung_AbsoluterFilePath_WirdDirektVerwendet` — absoluter Pfad → wird direkt verwendet

**Fehlende Testfälle (noch nicht vorhanden):** Sprachspezifische Datei vorhanden, Fallback-Szenario, neutrales Sprachkürzel.

---

### `ImpressumPageTests`
Datei: `src/Schnittstellenzentrale.Tests/Playwright/ImpressumPageTests.cs`

Playwright-Tests. Zwei Testklassen in derselben Datei:

**`ImpressumPageTests`** (`[Collection("Playwright")]`) — läuft gegen `PlaywrightServer` (ohne Impressum-Datei):
- `ImpressumSeite_ZeigtHinweis_WennDateiFehlt` — `/impressum` zeigt lokalisierten Hinweistext (`"No imprint available."` / `"Kein Impressum verfügbar."`) wenn Datei fehlt
- `SidebarFooter_LinkFehlt_WennDateiNichtVorhanden` — Impressum-Link im Sidebar-Footer ist nicht vorhanden, wenn Datei fehlt

**`ImpressumPageWithFileTests`** (`[Collection("PlaywrightImpressum")]`) — läuft gegen `PlaywrightImpressumServer` (mit Impressum-Datei):
- `ImpressumSeite_ZeigtInhalt_WennDateiVorhanden` — `/impressum` zeigt Überschrift und Markdown-Inhalt `"Dies ist ein Test-Impressum."`
- `SidebarFooter_LinkSichtbar_WennDateiVorhanden` — Impressum-Link im Sidebar-Footer ist sichtbar

**Fehlende Testfälle (noch nicht vorhanden):** Sprachauswahlverhalten über `Accept-Language`-Header (sprachspezifische Datei, Fallback).

---

## Hilfsmethoden / Infrastruktur

### `ImpressumServiceTests`
- `CreateService(string filePath)` — statische Hilfsmethode, erstellt `ImpressumService` mit `Options.Create(new ImpressumSettings { FilePath = filePath })`

### `PlaywrightImpressumServer`
Datei: `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightImpressumServer.cs`

- Erbt von `PlaywrightServer`, bindet auf Port **5101**
- `InitializeAsync()` — legt vor App-Start `AppContext.BaseDirectory/impressum.md` mit Inhalt `"# Impressum\n\nDies ist ein Test-Impressum."` an; löscht die Datei bei Fehler wieder
- `DisposeAsync()` — löscht die angelegte Impressum-Datei nach den Tests

### `PlaywrightImpressumCollection`
Datei: `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightImpressumCollection.cs`

- xUnit-Collection-Definition `[CollectionDefinition("PlaywrightImpressum")]` mit `ICollectionFixture<PlaywrightImpressumServer>`
