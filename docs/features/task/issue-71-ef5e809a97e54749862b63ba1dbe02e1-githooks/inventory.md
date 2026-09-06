# Bestandsaufnahme – GitHooks

## Quelle der Hooks

Repository `https://github.com/martin-stromberg/Pattern-Collection.git`,
Commit `9683e0c` (aktueller HEAD), Verzeichnis `Git-Hooks/githooks/`:

| Datei | Zweck |
|-------|-------|
| `pre-commit` | Branch-Blocker (main/staging), ruft alle Checks + optional SecretScan/MarkdownLinkCheck auf |
| `pre-push` | Push-Blocker (main/staging), ruft 3 Checks mit `--all --strict` auf |
| `translation-check.py` | resx-Konsistenz (Schlüssel, Paket-Vollständigkeit, Header) — blockierend |
| `csproj-xmldoc-check.py` | XML-Doku-Konfiguration in .csproj + Vollständigkeit in .cs — blockierend |
| `razor-l10n-check.py` | hartcodierte UI-Strings in .razor — blockierend |
| `razor-usage-check.py` | verwaiste Razor-Komponenten — Commit: Warnung, Push: blockierend |
| `no-notimplemented-check.py` | NotImplementedException/Throw-Stubs — Commit: Warnung, Push: blockierend |
| `enum-coverage-check.py` | Enum-Werte müssen in Testdateien vorkommen — Commit: Warnung, Push: blockierend |
| `install-hooks.cmd/.sh` | setzt `git config --local core.hooksPath .githooks` |

Die Skripte erwarten den Pfad `.githooks/` im Repo-Root
(`$repo_root/.githooks/*.py`).

## Ist-Zustand im Repo

- Kein `.githooks/`-Verzeichnis, kein `core.hooksPath` gesetzt, keine älteren
  Hook-Versionen versioniert. `.claude/hooks/` enthält Agent-Tool-Hooks
  (kein Bezug zu Git-Hooks, bleibt unberührt).
- `SecretScan.csproj` / `MarkdownLinkCheck.csproj` existieren nicht —
  die entsprechenden pre-commit-Abschnitte überspringen sich selbst.
- Lösung: `Schnittstellenzentrale.slnx` mit 4 Projekten
  (Haupt, Core, Infrastructure, Tests). Python 3.13 und .NET SDK 10.0.400
  sind installiert.

## Ergebnis der Checks (alle mit `--all` ausgeführt)

| Check | Ergebnis |
|-------|----------|
| translation-check | **OK** — 4 resx-Pakete konsistent, Header valide |
| razor-l10n-check | **OK** — 60 .razor-Dateien sauber |
| csproj-xmldoc-check | **FEHLER** — siehe unten |
| razor-usage-check (--strict) | **FEHLER** — 5 verwaiste Komponenten |
| no-notimplemented-check (--strict) | **FEHLER** — 1 Datei |
| enum-coverage-check (--strict) | **FEHLER** — 3 Enums |

### csproj-xmldoc-check: Befunde

1. `Schnittstellenzentrale.Core.csproj`: `GenerateDocumentationFile` und
   `CS1591`-als-Fehler fehlen (die anderen 3 Projekte sind korrekt
   konfiguriert).
2. `#pragma warning disable CS1591` in 5 Dateien:
   `Infrastructure/Services/ThemeService.cs`, `HealthCheckService.cs`,
   `WindowsCredentialService.cs`, `WindowsCurrentUserService.cs`,
   `Schnittstellenzentrale/Services/TokenStore.cs` +
   `Controllers/AuthController.cs` (insgesamt 6 Fundstellen in 5 Dateien
   lt. Log: ThemeService, WindowsCredentialService, WindowsCurrentUserService,
   HealthCheckService, AuthController, TokenStore).
   → Nach Entfernen des pragma müssen alle öffentlichen Member dieser
   Dateien XML-Kommentare erhalten (CS1591 ist Build-Fehler).
3. Unvollständige XML-Kommentare (fehlende `<param>`/`<typeparam>`/
   `<returns>`/`<response>`) in ca. 45 Dateien über alle 4 Projekte.
   Vollständige Liste: Ausgabe des Checks (in `inventory/xmldoc-findings.txt`).

### razor-usage-check: Befunde

- `Components/Pages/Error.razor`: hat `@page "/Error"`, wird aber wegen
  UTF-8-BOM nicht als Einstiegspunkt erkannt → BOM entfernen.
- `Components/Layout/AppShell.razor`: wird in `Routes.razor` als
  `typeof(Layout.AppShell)` referenziert; der Check erkennt nur
  `typeof(AppShell)` → Qualifizierung in `Routes.razor` anpassen
  (`@using Schnittstellenzentrale.Components.Layout` fehlt in `_Imports.razor`).
- `Components/Layout/MainLayout.razor`, `Components/Layout/NavMenu.razor`:
  ungenutzte Blazor-Template-Reste (keine Referenzen) → löschen.
- `Components/Shared/ActivityLogPanel.razor`: seit Redesign (71fd36f)
  unverdrahtet, vorher in `MainLayout` eingebettet. Aktivitätslog-Service
  existiert weiterhin → **Offener Punkt**: einbinden oder löschen.

### no-notimplemented-check: Befund

`src/Schnittstellenzentrale.Tests/Integration/SystemEntryInitializerTests.cs`
Zeilen 284–316: `ThrowingApplicationRepository` implementiert
`IApplicationRepository` mit 13 `NotImplementedException`-Stubs (nur
`GetSystemGroupAsync` soll werfen). → Restliche Member echt implementieren
(triviale `Task.FromResult`-Rückgaben).

### enum-coverage-check: Befunde

- `ActivityLogCategory`: `EntityMoved`, `ContextSwitched` ohne Testbezug
- `BodyMode`: `Xml`, `PlainText` ohne Testbezug
- `HttpMethod`: `HEAD`, `OPTIONS` ohne Testbezug
→ Tests ergänzen, die die fehlenden Werte verwenden.

## Konventionen (CLAUDE.md)

- ASP.NET Core 9/10 / Blazor Server, eigenes `.sz-*`-CSS-System.
- API-First: UI-Komponenten nur über `IApplicationApiClient`.
- Tests: `dotnet test --filter "FullyQualifiedName!~Playwright"`.
- resx-Konvention: ein Paket pro Projekt (`SharedResources`, `CoreResources`).
