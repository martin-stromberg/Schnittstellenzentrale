# Tests

## Testklassen

### `ApplicationContextMenuTests`
Datei: `src/Schnittstellenzentrale.Tests/Components/ApplicationContextMenuTests.cs`

- `AusGruppeEntfernen_NurSichtbar_WennAnwendungInGruppe` — Prüft Sichtbarkeit eines Menüeintrags anhand hartcodiertem deutschen Text `"Aus Sammlung entfernen"`
- `AusGruppeEntfernen_NichtSichtbar_WennAnwendungOhneGruppe` — Prüft Abwesenheit anhand hartcodiertem deutschen Text `"Aus Sammlung entfernen"`
- `AusGruppeEntfernen_LöstCallbackAus_UndSchliestMenu` — Klick auf deutschen Text `"Aus Sammlung entfernen"`, prüft Callback und Menü-Schließen
- `Bearbeiten_Deaktiviert_WennIsSystem` — Prüft disabled-Zustand anhand hartcodiertem deutschen Text `"Bearbeiten"`
- `Löschen_Deaktiviert_WennIsSystem` — Prüft disabled-Zustand anhand hartcodiertem deutschen Text `"Löschen"`

**Relevanz für Lokalisierung:** Alle fünf Tests suchen Buttons per `b.TextContent.Contains(...)` mit hartem deutschen Text. Nach Umstellung auf `IStringLocalizer` müssen diese Tests entweder einen Lokalisierungs-Mock einrichten oder die englische Standardsprache (Fallback) berücksichtigen.

### `EndpointContextMenuTests`
Datei: `src/Schnittstellenzentrale.Tests/Components/EndpointContextMenuTests.cs`

- `LöschenEintrag_LöstCallbackAus` — Klick auf deutschen Text `"Endpunkt löschen"`, prüft Callback

**Relevanz für Lokalisierung:** Enthält hartcodierten deutschen Text `"Endpunkt löschen"` als Suchkriterium.

### `EndpointGroupContextMenuTests`
Datei: `src/Schnittstellenzentrale.Tests/Components/EndpointGroupContextMenuTests.cs`

- `EndpunktAnlegen_LöstCallbackAus` — Klick auf deutschen Text `"Endpunkt anlegen"`
- `OrdnerUmbenennen_LöstCallbackAus` — Klick auf deutschen Text `"Ordner umbenennen"`
- `OrdnerLöschen_LöstCallbackAus` — Klick auf deutschen Text `"Ordner löschen"`

**Relevanz für Lokalisierung:** Alle drei Tests suchen Buttons per hartcodierten deutschen Texten.

### `MainLayoutTests`
Datei: `src/Schnittstellenzentrale.Tests/Components/MainLayoutTests.cs`

- `AppShell_RendertWorkspacesTab` — Prüft `TextContent.Contains("Workspaces")` (englischer Begriff, nicht betroffen)
- `AppShell_RendertEnvironmentsTab` — Prüft `TextContent.Contains("Environments")` (englischer Begriff, nicht betroffen)
- `AppShell_RendertHistoryTab` — Prüft `TextContent.Contains("History")` (englischer Begriff, nicht betroffen)
- `AppShell_RendertModusSelektor` — Prüft `select.sz-topbar-select`, kein Textabgleich
- `AppShell_RendertProfilIcon` — Prüft Initiale `"T"`, kein Sprachbezug
- `DisposeAsync_OhneHubConnection_WirftKeinenFehler` — Kein Textabgleich
- `Wiederherstellen_GespeicherteIdVorhanden_SetzAktiveUmgebung` — Kein Textabgleich
- `Wiederherstellen_UmgebungNichtMehrInDb_BereinigTLocalStorage` — Kein Textabgleich
- `Wiederherstellen_KeinEintragImLocalStorage_SetzNichts` — Kein Textabgleich
- `AppShell_SetAreaAsync_AktualisiertBereich` — Kein Textabgleich
- `Wiederherstellen_BeiModuswechsel_VerwendetNeuenSchlüssel` — Prüft `select.sz-topbar-select` per `.Change(StorageMode.User.ToString())`, kein Sprachbezug

**Relevanz für Lokalisierung:** Tabs `"Workspaces"`, `"Environments"`, `"History"` sind bereits englisch und werden vom Lokalisierungsfeature nicht umbenannt. Der Test für den Select sucht per `StorageMode.User.ToString()` (Enum-Wert, sprachunabhängig). Kein Handlungsbedarf für diese Testklasse, außer `IStringLocalizer`-Registration in der Test-Services-Konfiguration, falls die Komponenten darauf zugreifen.

### `EnvironmentSelectorTests`
Datei: `src/Schnittstellenzentrale.Tests/Components/EnvironmentSelectorTests.cs`

- `RendertUmgebungenAusRepository` — Prüft Optionen per Umgebungsname (`"Dev"`, `"Prod"`), kein Sprachtext
- `AktiveUmgebungWirdVorausgewählt` — Prüft `value`-Attribut, kein Sprachtext
- `OhneAktiveUmgebung_ZeigtKeineVorauswahl` — Kein Textabgleich
- `RefreshAsync_AktualistertListeOhneFehler` — Prüft `Single(cut.FindAll("option"))` — enthält implizit die Option `"— Keine Umgebung —"` (hartcodierter deutschen Text)
- `AuswählenEinerUmgebung_SchreibtLocalStorage` — Kein Textabgleich
- `AbwählenEinerUmgebung_EntferntLocalStorage` — Kein Textabgleich
- `AuswählenNichtExistierenderId_EntferntLocalStorageUndSetztNull` — Kein Textabgleich

**Relevanz für Lokalisierung:** `RefreshAsync_AktualistertListeOhneFehler` geht davon aus, dass genau eine Option existiert (implizit der Platzhalter-Text `"— Keine Umgebung —"`). Falls dieser Text lokalisiert wird, ist der Test von Lokalisierungs-Mocking abhängig.

### `EndpointPageTests`
Datei: `src/Schnittstellenzentrale.Tests/Components/EndpointPageTests.cs`

Kein Textabgleich auf deutschen Strings in den sichtbaren Testmethoden. Services werden gemockt, kein `IStringLocalizer` registriert.

## Hilfsmethoden

### `TestMockFactory`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/TestMockFactory.cs`

- `CreateActivityLogServiceMock()` — Erstellt leeren `IActivityLogService`-Mock
- `CreateEnv(int id, string name)` — Erstellt `SystemEnvironment`-Testinstanz

Kein Bezug zu Lokalisierung.

## Integrationstests

Datei: `src/Schnittstellenzentrale.Tests/Integration/`

Keine der vorhandenen Integrationstest-Klassen (`ApplicationApiClientIntegrationTests`, `ApplicationGroupsControllerIntegrationTests`, etc.) enthält eine `LocalizationTests`-Klasse. Eine `WebApplicationFactory`-basierte Testklasse für Accept-Language-Verhalten existiert nicht.

## Playwright-Tests

Datei: `src/Schnittstellenzentrale.Tests/Playwright/`

- `LayoutSmokeTests` — Prüft per `HasText` die Tab-Texte `"Workspaces"`, `"Environments"`, `"History"` (alle englisch, nicht betroffen). Kein Textabgleich auf deutsche Strings.
- Übrige Playwright-Tests (`ApplicationCrudTests`, `GroupCrudTests`, etc.) — nicht auf Lokalisierungskontext hin analysiert, da sie primär Interaktionsflüsse testen.
