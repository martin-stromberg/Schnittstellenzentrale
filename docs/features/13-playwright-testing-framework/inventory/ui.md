# Detailanalyse: UI-Struktur (Blazor-Komponenten)

## Startseite (`Home.razor`)
Datei: `src/Schnittstellenzentrale/Components/Pages/Home.razor`

Einzige nicht-triviale Seite der Anwendung unter Route `/`. Enthält das gesamte Haupt-UI:
- `ApplicationGroupTree` (linke Sidebar) — Baum aller Gruppen, Anwendungen, Endpunkte
- Content-Pane (rechts) — zeigt kontextabhängig Editoren, Dialoge oder die `EndpointPage`

Die Sidebar-Breite wird über JavaScript (`endpoint-page.js`) gespeichert und wiederhergestellt.

---

## `MainLayout.razor`
Datei: `src/Schnittstellenzentrale/Components/Layout/MainLayout.razor`

Enthält eine `<select>`-Dropdown für den Speichermodus (`StorageMode.Team` / `StorageMode.User`). Aufruf von `StorageModeService.SetMode()` bei Änderung. CSS-Klasse `top-row` mit `d-flex`. Für Playwright relevant: Modus-Wechsel erfolgt über dieses Dropdown.

---

## `ApplicationGroupTree.razor`
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationGroupTree.razor`

Zentrale Baumkomponente. Lädt Daten via `IApplicationApiClient.GetGroupsAsync()` und `GetUngroupedApplicationsAsync()`. Zeigt CollapsibleSections pro Gruppe mit Anwendungen, Endpunktgruppen und Endpunkten.

Relevant für Playwright-Tests:
- `RefreshAsync()` — public Methode zum Neu-Laden (wird vom `Home.razor` nach Änderungen aufgerufen)
- `ExpandApplicationAsync(int)` — blendet Endpunkte einer Anwendung ein
- Verbindet sich per `HubConnectionBuilder` mit `/hubs/endpoint` für Live-Updates
- Empfängt `ApplicationChanged`, `GroupChanged`, `EndpointChanged`, `EndpointGroupChanged`-Events
- Buttons: „Neue Gruppe", „Neue Anwendung" (oben im Baum)
- Drag & Drop zum Verschieben von Anwendungen zwischen Gruppen

---

## `ApplicationCard.razor`
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationCard.razor`

Zeigt Details einer ausgewählten Anwendung. Enthält Buttons:
- „Swagger-Import" (nur für `InterfaceType.Rest`)
- „OData-Import" (nur für `InterfaceType.OData`)
- „Health-Check" (immer)

Öffnet bei Klick die jeweiligen Dialoge.

---

## `ApplicationEditor.razor`
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationEditor.razor`

Formular zum Anlegen und Bearbeiten einer Anwendung. Felder: Name, Basis-URL, Beschreibung, Schnittstellen-URL (mit automatischer Erkennung des `InterfaceType`). Wird im Content-Pane von `Home.razor` als Modal-artige Einblendung angezeigt.

---

## `SwaggerImportDialog.razor`
Datei: `src/Schnittstellenzentrale/Components/Shared/SwaggerImportDialog.razor`

Thin Wrapper um `ImportDialog`. Empfängt `ImportDiff` und `Application` als Parameter. Ruft `SwaggerImportService.ApplyDiffAsync()` bei Bestätigung auf. Das eigentliche Öffnen des Dialogs (inkl. Laden des Diff via `SwaggerImportService.ImportAsync()`) erfolgt in `ApplicationCard.razor`.

---

## `HealthCheckDialog.razor`
Datei: `src/Schnittstellenzentrale/Components/Shared/HealthCheckDialog.razor`

Zeigt Ergebnis des Health-Checks:
- `null` → „Health-Check wurde übersprungen (Cooldown aktiv)"
- `true` → „Die Anwendung ist erreichbar" (Bootstrap `alert-success`)
- `false` → „Die Anwendung ist nicht erreichbar" (Bootstrap `alert-danger`) + Button „Anwendung entfernen"

Parameter: `Application`, `IsReachable` (`bool?`), `OnClose`, `OnRemove`.

---

## `EndpointPage.razor`
Datei: `src/Schnittstellenzentrale/Components/Shared/EndpointPage.razor`

UI zum Betrachten, Bearbeiten und Ausführen eines Endpunkts. Enthält:
- Name-Eingabe, Method-Select, Pfad-Eingabe
- Tab-Navigation: Autorisierung / Headers / Query-Parameter / Body
- Button „Anfrage senden" → `SendRequestAsync()` → `ExecutionService.ExecuteAsync()`
- Response-Bereich mit Status, Headers, Body

Für `EndpointExecutionTests` relevant: Antwort erscheint nach Klick auf „Anfrage senden".

---

## Speichermodus-Umschaltung

Der Modus wird in `MainLayout.razor` über ein `<select>`-Element mit `value="@StorageModeService.CurrentMode"` gesteuert. Optionen: `Team` und `Benutzer` (Wert: `User`). Nach Moduswechsel löst `StorageModeService.OnModeChanged` aus, was `ApplicationGroupTree` veranlasst, die Daten neu zu laden und die Auswahl zu löschen.
