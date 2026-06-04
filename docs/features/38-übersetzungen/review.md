# Plan-Review

## Ergebnis

**Status:** Vollständig umgesetzt

## Umgesetzte Planelemente

### Neue Klassen

- [x] `SharedResources` (Marker-Klasse) — angelegt unter `src/Schnittstellenzentrale/Resources/SharedResources.cs`
- [x] `CoreResources` (Marker-Klasse) — angelegt unter `src/Schnittstellenzentrale.Core/Resources/CoreResources.cs`
- [x] `LocalizationTests` (xUnit-Integrationstest) — angelegt unter `src/Schnittstellenzentrale.Tests/Integration/LocalizationTests.cs`

### Neue resx-Dateien

- [x] `SharedResources.resx` (EN-Fallback) — vorhanden mit vollständig befüllten Schlüsseln, EN-Texten und Comment-Feldern
- [x] `SharedResources.de.resx` (DE) — vorhanden mit vollständig befüllten deutschen Übersetzungen und Comments; enthält auch DataAnnotations-Standardschlüssel auf Deutsch
- [x] `CoreResources.resx` — vorhanden; enthält keine Einträge (plankonform, da DataAnnotations-Provider auf `SharedResources` zeigt)
- [x] `CoreResources.de.resx` — vorhanden; enthält keine Einträge (plankonform)

### `Program.cs` — Middleware-Konfiguration

- [x] `AddLocalization()` — registriert
- [x] `AddDataAnnotationsLocalization()` mit Provider auf `SharedResources` — registriert
- [x] `UseRequestLocalization()` mit `DefaultCulture: "en"` und `SupportedCultures: ["en", "de"]` — registriert; steht nach `UseAuthorization`, vor `UseAntiforgery`

### `_Imports.razor`

- [x] `@using Microsoft.Extensions.Localization` — eingetragen

### `CLAUDE.md`

- [x] Abschnitt „Lokalisierung (resx-Konvention)" — hinzugefügt mit Paketstruktur, Schlüsselschema (`{KomponentenName}_{Rolle}`), Comment-Pflicht und `@inject`-Konvention

### Razor-Komponenten — Injektion und Textersatz

- [x] `AppShell.razor` — Injektion vorhanden; `AppShell_ErrorMessage` und `AppShell_ReloadLink` im `#blazor-error-ui`-Div verwendet
- [x] `TopBar.razor` — Injektion vorhanden; Modus-Label, Team/User-Optionen, Tabs (Workspaces/Environments/History), Tooltips (Settings/Help) durch `@L["TopBar_..."]` ersetzt
- [x] `EmptyContentView.razor` — Injektion vorhanden; Hinweistext durch `@L["EmptyContentView_Hint"]` ersetzt
- [x] `ApplicationEditor.razor` — Injektion vorhanden; Titel, Labels, Optionen, Hints, Buttons, Fehlermeldungen durch `@L["ApplicationEditor_..."]` ersetzt
- [x] `ApplicationGroupEditor.razor` — Injektion vorhanden; Titel, Label, Buttons, Fehlermeldungen durch `@L["ApplicationGroupEditor_..."]` ersetzt
- [x] `ApplicationContextMenu.razor` — Injektion vorhanden; alle fünf Menüeinträge durch `@L["ApplicationContextMenu_..."]` ersetzt
- [x] `ApplicationGroupContextMenu.razor` — Injektion vorhanden; Umbenennen/Löschen durch `@L["ApplicationGroupContextMenu_..."]` ersetzt
- [x] `EndpointContextMenu.razor` — Injektion vorhanden; „Endpunkt löschen" durch `@L["EndpointContextMenu_DeleteButton"]` ersetzt
- [x] `EndpointGroupContextMenu.razor` — Injektion vorhanden; alle drei Einträge durch `@L["EndpointGroupContextMenu_..."]` ersetzt
- [x] `ConfirmDeleteGroupDialog.razor` — Injektion vorhanden; Titel, Nachrichten, Buttons durch `@L["ConfirmDeleteGroupDialog_..."]` ersetzt
- [x] `ConfirmDeleteApplicationDialog.razor` — Injektion vorhanden; Titel, Nachricht, Buttons durch `@L["ConfirmDeleteApplicationDialog_..."]` ersetzt
- [x] `ConfirmDeleteEndpointGroupDialog.razor` — Injektion vorhanden; Titel, Nachrichten, Buttons durch `@L["ConfirmDeleteEndpointGroupDialog_..."]` ersetzt
- [x] `RenameGroupDialog.razor` — Injektion vorhanden; Titel, Label, Buttons, Fehlermeldung durch `@L["RenameGroupDialog_..."]` ersetzt
- [x] `RenameEndpointGroupDialog.razor` — Injektion vorhanden; Titel, Label, Buttons, Inline-Validierung, Fehlermeldung durch `@L["RenameEndpointGroupDialog_..."]` ersetzt
- [x] `CreateEndpointGroupDialog.razor` — Injektion vorhanden; Titel, Label, Buttons, Inline-Validierung, Fehlermeldung durch `@L["CreateEndpointGroupDialog_..."]` ersetzt
- [x] `ConcurrencyWarningDialog.razor` — Injektion vorhanden; Titel, Nachricht, Buttons durch `@L["ConcurrencyWarningDialog_..."]` ersetzt
- [x] `EndpointPage.razor` — Injektion vorhanden; Badge, Buttons, Inline-Validierung, Tabs, Response-Labels, Placeholders, confirm()-Dialog, Fehlermeldungen durch `@L["EndpointPage_..."]` ersetzt
- [x] `ImportDialog.razor` — Injektion vorhanden; Status-Texte, Buttons, Fehlermeldungen durch `@L["ImportDialog_..."]` ersetzt
- [x] `HealthCheckDialog.razor` — Injektion vorhanden; Status-Meldungen, Buttons durch `@L["HealthCheckDialog_..."]` ersetzt
- [x] `EnvironmentEditor.razor` — Injektion vorhanden; Sektion, Tabellen-Header, Leerzustand, Tooltips, Buttons, Inline-Validierungen, Fehlermeldungen durch `@L["EnvironmentEditor_..."]` ersetzt
- [x] `EnvironmentManagementOverlay.razor` — Injektion vorhanden; Titel, Bestätigungsnachricht, Buttons, Leerzustand, Fehlermeldung durch `@L["EnvironmentManagementOverlay_..."]` ersetzt
- [x] `EnvironmentSelector.razor` — Injektion vorhanden; Platzhalter-Option durch `@L["EnvironmentSelector_NoEnvironmentOption"]` ersetzt
- [x] `EnvironmentsSidebar.razor` — Injektion vorhanden; Button, Tooltip, Placeholder, Buttons durch `@L["EnvironmentsSidebar_..."]` ersetzt
- [x] `WorkspacesSidebar.razor` — Injektion vorhanden; Buttons, Footer-Link durch `@L["WorkspacesSidebar_..."]` ersetzt
- [x] `LinksManager.razor` — Injektion vorhanden; Sektion, Button, Placeholders, Buttons durch `@L["LinksManager_..."]` ersetzt
- [x] `RequestAuthPanel.razor` — Injektion vorhanden; Labels, Hints, Placeholders durch `@L["RequestAuthPanel_..."]` ersetzt

### Tests — Neue Testmethoden und Hilfsmethoden

- [x] `DeRequestMitAcceptLanguageDe_ZeigtDeutscheTexte` in `LocalizationTests` — vorhanden; prüft deutschen Text bei `Accept-Language: de`
- [x] `DeRequestMitAcceptLanguageEn_ZeigtEnglischeTexte` in `LocalizationTests` — vorhanden; prüft englischen Text bei `Accept-Language: en`
- [x] `DeRequestOhneAcceptLanguage_ZeigtEnglischeTexte` in `LocalizationTests` — vorhanden; prüft Fallback ohne Header
- [x] `DeRequestMitUnbekannterSprache_ZeigtEnglischeTexte` in `LocalizationTests` — vorhanden; prüft Fallback bei `Accept-Language: fr`
- [x] `CreateFakeLocalizer()` in `TestMockFactory` — vorhanden; gibt jeden Schlüssel unverändert als Wert zurück; Format-Overload mit `string.Format` ebenfalls implementiert

### Tests — Angepasste bestehende Tests

- [x] `ApplicationContextMenuTests` (alle 5 Tests) — `FakeLocalizer` im Konstruktor registriert; Suchausdrücke auf Ressourcen-Schlüssel umgestellt
- [x] `EndpointContextMenuTests.LöschenEintrag_LöstCallbackAus` — `FakeLocalizer` registriert; sucht per Ressourcen-Schlüssel `"EndpointContextMenu_DeleteButton"`
- [x] `EndpointGroupContextMenuTests` (alle 3 Tests) — `FakeLocalizer` im Konstruktor registriert; alle Suchausdrücke auf Schlüssel umgestellt
- [x] `EnvironmentSelectorTests.RefreshAsync_AktualistertListeOhneFehler` — `FakeLocalizer` im Konstruktor registriert
- [x] `MainLayoutTests` (alle Tests, die `AppShell` rendern) — `FakeLocalizer` im Konstruktor registriert

## Offene Aufgaben

Keine.

## Hinweise

- `EndpointPageTests` war im Plan nicht unter den „betroffenen Tests" aufgeführt, wurde aber ebenfalls mit `FakeLocalizer` ausgestattet, da `EndpointPage` jetzt `IStringLocalizer<SharedResources>` injiziert. Das ist eine korrekte Zusatzmaßnahme.
- `CoreResources.resx` und `CoreResources.de.resx` sind vorhanden, enthalten jedoch keine Daten-Einträge. Das ist plankonform: Der Plan sieht die Dateien als Artefakt vor; die DataAnnotations-Standard-Keys (`The field {0} is required.` u. a.) wurden plangemäß in `SharedResources.de.resx` eingetragen und der Provider in `AddDataAnnotationsLocalization()` zeigt auf `SharedResources`.
- Die `AddDataAnnotationsLocalization()`-Konfiguration zeigt ausschließlich auf `SharedResources`, nicht auf `CoreResources`. Das entspricht dem im Plan dokumentierten Designentscheid und dem Hinweis unter „Offene Punkte #1".
