# Plan-Review

## Ergebnis

**Status:** Vollständig umgesetzt

## Umgesetzte Planelemente

### Neue Klassen

- [x] `SharedResources` (Marker-Klasse) — angelegt unter `src/Schnittstellenzentrale/Resources/SharedResources.cs`
- [x] `CoreResources` (Marker-Klasse) — angelegt unter `src/Schnittstellenzentrale.Core/Resources/CoreResources.cs`
- [x] `LocalizationTests` (xUnit-Integrationstest) — angelegt unter `src/Schnittstellenzentrale.Tests/Integration/LocalizationTests.cs`

### Neue resx-Dateien

- [x] `SharedResources.resx` (EN-Fallback) — vorhanden
- [x] `SharedResources.de.resx` (DE) — vorhanden
- [x] `CoreResources.resx` — vorhanden
- [x] `CoreResources.de.resx` — vorhanden

### `Program.cs` (Middleware-Konfiguration)

- [x] `AddLocalization()` — registriert (Zeile 125)
- [x] `AddDataAnnotationsLocalization()` mit Provider auf `SharedResources` — registriert (Zeilen 52–54)
- [x] `UseRequestLocalization()` mit DefaultCulture `"en"` und SupportedCultures `["en", "de"]` — registriert (Zeilen 149–153)

### `_Imports.razor`

- [x] `@using Microsoft.Extensions.Localization` — eingetragen

### Razor-Komponenten (Injektion + Textersatz)

- [x] `AppShell.razor` — `@inject IStringLocalizer<SharedResources> L` vorhanden; `AppShell_ErrorMessage` und `AppShell_ReloadLink` verwendet
- [x] `TopBar.razor` — Injektion vorhanden; alle deutschen Texte (Modus-Label, Team/Benutzer, Tabs, Tooltips) durch `@L["TopBar_..."]` ersetzt
- [x] `ApplicationEditor.razor` — Injektion vorhanden; alle hartcodierten deutschen Texte durch `@L["ApplicationEditor_..."]` ersetzt
- [x] `ApplicationGroupEditor.razor` — Injektion vorhanden; Texte durch `@L["ApplicationGroupEditor_..."]` ersetzt
- [x] `ApplicationContextMenu.razor` — Injektion vorhanden; Texte durch `@L["ApplicationContextMenu_..."]` ersetzt
- [x] `ApplicationGroupContextMenu.razor` — Injektion vorhanden; Texte durch `@L["ApplicationGroupContextMenu_..."]` ersetzt
- [x] `EndpointContextMenu.razor` — Injektion vorhanden; Text durch `@L["EndpointContextMenu_DeleteButton"]` ersetzt
- [x] `EndpointGroupContextMenu.razor` — Injektion vorhanden; Texte durch `@L["EndpointGroupContextMenu_..."]` ersetzt
- [x] `ConfirmDeleteGroupDialog.razor` — Injektion vorhanden; Texte durch `@L["ConfirmDeleteGroupDialog_..."]` ersetzt
- [x] `ConfirmDeleteApplicationDialog.razor` — Injektion vorhanden; Texte durch `@L["ConfirmDeleteApplicationDialog_..."]` ersetzt
- [x] `ConfirmDeleteEndpointGroupDialog.razor` — Injektion vorhanden; Texte durch `@L["ConfirmDeleteEndpointGroupDialog_..."]` ersetzt
- [x] `RenameGroupDialog.razor` — Injektion vorhanden; Texte durch `@L["RenameGroupDialog_..."]` ersetzt
- [x] `RenameEndpointGroupDialog.razor` — Injektion vorhanden; Texte durch `@L["RenameEndpointGroupDialog_..."]` ersetzt
- [x] `CreateEndpointGroupDialog.razor` — Injektion vorhanden; Texte durch `@L["CreateEndpointGroupDialog_..."]` ersetzt
- [x] `ConcurrencyWarningDialog.razor` — Injektion vorhanden; Texte durch `@L["ConcurrencyWarningDialog_..."]` ersetzt
- [x] `EndpointPage.razor` — Injektion vorhanden; alle deutschen Texte (Badge, Buttons, Tabs, Response-Labels, Placeholders, Inline-Validierung) durch `@L["EndpointPage_..."]` ersetzt
- [x] `ImportDialog.razor` — Injektion vorhanden; Texte durch `@L["ImportDialog_..."]` ersetzt
- [x] `HealthCheckDialog.razor` — Injektion vorhanden; Texte durch `@L["HealthCheckDialog_..."]` ersetzt
- [x] `EnvironmentEditor.razor` — Injektion vorhanden; alle deutschen Texte durch `@L["EnvironmentEditor_..."]` ersetzt
- [x] `EnvironmentManagementOverlay.razor` — Injektion vorhanden; Texte durch `@L["EnvironmentManagementOverlay_..."]` ersetzt
- [x] `EnvironmentSelector.razor` — Injektion vorhanden; Platzhalter-Option durch `@L["EnvironmentSelector_NoEnvironmentOption"]` ersetzt
- [x] `EnvironmentsSidebar.razor` — Injektion vorhanden; Texte durch `@L["EnvironmentsSidebar_..."]` ersetzt
- [x] `WorkspacesSidebar.razor` — Injektion vorhanden; Texte durch `@L["WorkspacesSidebar_..."]` ersetzt
- [x] `EmptyContentView.razor` — Injektion vorhanden; Text durch `@L["EmptyContentView_Hint"]` ersetzt
- [x] `LinksManager.razor` — Injektion vorhanden; Texte durch `@L["LinksManager_..."]` ersetzt
- [x] `RequestAuthPanel.razor` — Injektion vorhanden; Texte durch `@L["RequestAuthPanel_..."]` ersetzt

### `CLAUDE.md`

- [x] Abschnitt „Lokalisierung (resx-Konvention)" — hinzugefügt mit Beschreibung der Paketstruktur, Schlüsselschema und Comment-Pflicht

### Tests: Neue Testmethoden

- [x] `DeRequestMitAcceptLanguageDe_ZeigtDeutscheTexte` in `LocalizationTests` — vorhanden
- [x] `DeRequestMitAcceptLanguageEn_ZeigtEnglischeTexte` in `LocalizationTests` — vorhanden
- [x] `DeRequestOhneAcceptLanguage_ZeigtEnglischeTexte` in `LocalizationTests` — vorhanden
- [x] `DeRequestMitUnbekannterSprache_ZeigtEnglischeTexte` in `LocalizationTests` — vorhanden
- [x] `CreateFakeLocalizer()` in `TestMockFactory` — vorhanden; gibt Schlüssel als Wert zurück

### Tests: Angepasste bestehende Tests

- [x] `ApplicationContextMenuTests` (alle 5 Tests) — `FakeLocalizer` registriert; Suchausdrücke auf Ressourcen-Schlüssel umgestellt
- [x] `EndpointContextMenuTests.LöschenEintrag_LöstCallbackAus` — `FakeLocalizer` registriert; sucht per `"EndpointContextMenu_DeleteButton"`
- [x] `EndpointGroupContextMenuTests` (alle 3 Tests) — `FakeLocalizer` registriert; Suchausdrücke auf Schlüssel umgestellt
- [x] `EnvironmentSelectorTests.RefreshAsync_AktualistertListeOhneFehler` — `FakeLocalizer` in Konstruktor registriert
- [x] `MainLayoutTests` — `FakeLocalizer` in Konstruktor registriert (Zeile 57)

## Offene Aufgaben

Keine.

## Hinweise

- `AddDataAnnotationsLocalization()` verwendet laut Code `SharedResources` als Provider (nicht `CoreResources`). Das entspricht dem Plan (Designentscheidungen-Tabelle: „zentraler Provider auf `SharedResources`"); `CoreResources` dient ausschließlich als Marker-Klasse für allfällige direkte `IStringLocalizer<CoreResources>`-Nutzung.
- `EndpointPageTests` erhält ebenfalls `CreateFakeLocalizer()` (Zeile 36), was der Plan als implizit notwendig eingestuft hat; dies ist eine korrekte Ergänzung über den Plan hinaus.
