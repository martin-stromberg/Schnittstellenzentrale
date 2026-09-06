# Umsetzungsplan – GitHooks

Eingaben: `requirement.md`, `inventory.md` (+ `inventory/xmldoc-findings.txt`).

Die Prüfskripte werden **unverändert** aus dem Pattern-Collection-Repo
übernommen (Commit `9683e0c`). Alle Fehler werden in der Anwendung behoben.

## Arbeitspakete

### WP1: Hooks übernehmen und aktivieren
- Verzeichnis `Git-Hooks/githooks/` aus dem Quell-Repo als `.githooks/`
  in den Repo-Root kopieren (alle 10 Dateien).
- `git config --local core.hooksPath .githooks` setzen (entspricht
  `install-hooks.cmd`).
- `.gitignore` prüfen: `.githooks/` muss versionierbar bleiben.
- Keine alten Hook-Versionen vorhanden — nichts zu ersetzen.

### WP2: csproj-Konfiguration (Core)
- `Schnittstellenzentrale.Core.csproj`: `GenerateDocumentationFile=true`
  und `WarningsAsErrors` um `CS1591` ergänzen (analog zu den anderen
  Projekten). Danach muss `dotnet build` fehlerfrei bleiben — ggf. fehlende
  `<summary>`-Kommentare in Core ergänzen (CS1591-Fehlerliste aus dem Build).

### WP3: XML-Dokumentation vervollständigen
- Alle Befunde aus `inventory/xmldoc-findings.txt` abarbeiten:
  fehlende `<param>`-, `<typeparam>`-, `<returns>`-, `<response>`-Tags
  ergänzen (deutsch, im Stil der vorhandenen Kommentare).
- Die 5 `#pragma warning disable CS1591`-Zeilen entfernen und die
  betroffenen Dateien vollständig dokumentieren:
  - `Infrastructure/Services/ThemeService.cs`
  - `Infrastructure/Services/HealthCheckService.cs`
  - `Infrastructure/Services/WindowsCredentialService.cs`
  - `Infrastructure/Services/WindowsCurrentUserService.cs`
  - `Controllers/AuthController.cs`
  - `Services/TokenStore.cs`
- Aufteilung nach Projekt: Core, Infrastructure, Hauptprojekt, Tests.
- Nach jedem Teil: `dotnet build` muss grün bleiben.

### WP4: Verwaiste Razor-Komponenten
- `Components/Pages/Error.razor`: UTF-8-BOM entfernen (damit `@page`
  erkannt wird).
- `Components/Routes.razor`: `typeof(Layout.AppShell)` → `typeof(AppShell)`
  und `Schnittstellenzentrale.Components.Layout` in `_Imports.razor`
  importieren.
- `Components/Layout/MainLayout.razor` und `Components/Layout/NavMenu.razor`:
  löschen (ungenutzte Template-Reste). Zugehörige CSS-/resx-Reste prüfen.
- `Components/Shared/ActivityLogPanel.razor`: wieder einbinden
  (Entscheidung des Anwenders), analog zum Stand vor dem Redesign:
  - `TopBar.razor`: Icon-Button zum Ein-/Ausblenden des Protokolls
    (EventCallback `OnActivityLogToggleRequested`, resx-Schlüssel
    `TopBar_Tooltip_ActivityLog` in `SharedResources.resx` + `.de.resx`
    mit Kommentar).
  - `AppShell.razor`: State `_activityLogOpen`, `_activityLogDisplayMode`,
    `_activityLogPanelHeight`; Panel nur rendern, wenn geöffnet;
    `OnDisplayModeChanged`/`OnPanelHeightChanged` verdrahten;
    `padding-bottom` auf `.sz-app-content` bei dock-Modus; Persistenz der
    Anzeige-Einstellungen übernimmt das Panel selbst via JS/localStorage.
  - CSS: `.sz-app-content` bleibt scrollbar; Panel liegt fixiert am
    unteren Rand (bestehende `.activity-log-panel`-Styles verwenden).
  - Tests: bUnit-Test in `AppShellTests`/`MainLayoutTests` (Panel erscheint
    nach Toggle) und Playwright-Smoke-Test (Klick auf Protokoll-Button →
    `.activity-log-panel` sichtbar).

### WP5: Throw-Stubs im Testprojekt
- `SystemEntryInitializerTests.cs`: `ThrowingApplicationRepository` —
  die 13 `NotImplementedException`-Member durch echte triviale
  Implementierungen ersetzen (z. B. `Task.FromResult`-Rückgaben /
  leere Listen). Testverhalten unverändert lassen, Tests grün.

### WP6: Enum-Testabdeckung
- `ActivityLogCategory`: `EntityMoved`, `ContextSwitched` in Tests verwenden.
- `BodyMode`: `Xml`, `PlainText` in Tests verwenden.
- `HttpMethod`: `HEAD`, `OPTIONS` in Tests verwenden.
- Jeweils fachlich sinnvolle Tests (z. B. Mapping-Tests in
  `SwaggerOperationHelper`/`BodyMode`-Content-Type-Tests), nicht nur
  reine Namensnennung.

### WP7: Verifikation
- Alle 6 Prüfskripte im `--all`-Modus (bzw. `--all --strict`) ausführen —
  alle müssen mit Exit 0 enden.
- `dotnet build` und `dotnet test --filter "FullyQualifiedName!~Playwright"`
  grün.
- Abschluss-Commit löst den installierten pre-commit-Hook erfolgreich aus.

## Offene Punkte

Keine — der einzige offene Punkt (`ActivityLogPanel.razor`) wurde vom
Anwender entschieden: **wieder einbinden** (siehe WP4).

## Abweichung

Die im Lifecycle vorgesehenen Unterkommandos (`/translate-requirements`,
`/inventory`, `/plan`, `/plan-check`, `/implement`, `/review-*`,
`/run-tests`, `/update-*`) existieren in dieser Umgebung nicht. Die Schritte
werden daher vom Hauptagenten bzw. generischen Subagenten ausgeführt.
