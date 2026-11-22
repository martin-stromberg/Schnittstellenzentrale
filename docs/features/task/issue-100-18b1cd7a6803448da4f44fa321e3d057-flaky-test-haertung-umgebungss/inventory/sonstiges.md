# Sonstiger Bestand

## `scripts/` — nur Release-/Site-Skripte, keine Test-Helfer

| Datei | Zweck (aus Namen/Inhalt) |
|---|---|
| `scripts/generate-update-manifest.mjs` (+ `.test.mjs`) | Update-Manifest für Releases |
| `scripts/resolve-release-version.mjs` (+ `.test.mjs`) | Versionsauflösung |
| `scripts/verify-release-version.cjs` | Release-Version-Verifikation |
| `scripts/verify-site.mjs` (+ `.test.mjs`) | Prüfung der `site/`-Präsentationsseite |

**Fakt:** Kein Test-Runner-, TRX-Auswertungs- oder Flake-Statistik-Skript vorhanden.

## `docs/help/playwright-tests/` — vorhanden, aber inhaltlich veraltet

Dateien: `index.md`, `beschreibung.md`, `ablauf-technisch.md`, `installation.md`, `business-rules.md`.

Diskrepanzen Doku ↔ Code (Fakten, keine Bewertung):

| Doku-Aussage | Ist-Stand im Code |
|---|---|
| „`WebApplicationFactory<Program>` als echter Kestrel-HTTP-Server", Klassen `PlaywrightTestFactory`, `PlaywrightSignalRFactory` (`beschreibung.md`, `ablauf-technisch.md`) | Tatsächlich `PlaywrightServer` (eigener `WebApplication`-Kestrel) + `PlaywrightApiFactory` (In-Process-TestServer); Server-Hierarchie mit 6 Fixtures/Collections |
| „dedizierte SQLite-Datei `schnittstellenzentrale-tests.db`" (`beschreibung.md`, `installation.md` Konfigurationstabelle) | Benannte In-Memory-SQLite `Data Source={guid};Mode=Memory;Cache=Shared` (`PlaywrightServer.cs:63–66`); `schnittstellenzentrale-playwright.db` steht nur noch in `appsettings.Playwright.json` |
| „elf Playwright-Tests verteilen sich auf sieben Testklassen", Tabelle mit 7 Klassen (`beschreibung.md`, `installation.md` Verifikation) | 24 Testklassen in 22 Dateien |
| „Chromium headless `true` fest codiert" (`installation.md`) | `Headless = TestPaths.IsHeadless()` — headless nur bei `CI=true` (`TestPaths.cs:14–18`), lokal sichtbarer Browser |
| Trace-Tabelle nennt `SignalRSyncTests-A.zip`/`B.zip` (`installation.md`) | Tatsächlich `{TestName}.zip` pro Klasse (Hauptkontext) + `SignalRSyncTests-B.zip` (Zweitkontext) |
| Beispiel-Upload `actions/upload-artifact@v4` mit Pfad `bin/Debug/...` (`installation.md`) | CI nutzt `upload-artifact@v7` mit `playwright-traces/` im Repo-Root |
| Filter-Konvention `FullyQualifiedName~Playwright` (`installation.md` Ende) | Aktuell korrekt; Ziel des Issues ist Umstieg auf `Category=OsInterface` |

## `docs/help/stabilitaet/` — existiert nicht

Kein Verzeichnis `docs/help/stabilitaet/` (Blaupause-Verweis `docs/help/stabilitaet/os-interface-tests.md` aus Softwareschmiede PR #432 hat hier kein Gegenstück). Vorhandene `docs/help/*`-Themen: aktivitaetsprotokoll, anwendungen, api, dark-mode, endpunkte, git-hooks, github-page, impressum, lokalisierung, playwright-tests, review-automatisierung, schnittstellenzentrale, speichermodus, systemumgebungen.

## `CLAUDE.md` — Testkonventionen (Zeilen 20–28)

```
- Unit-Tests: dotnet test --filter "FullyQualifiedName!~Playwright"
- Playwright läuft über PlaywrightServer (Kestrel, Port 5099) — kein manueller App-Start nötig
- LayoutSmokeTests prüft via BoundingBoxAsync() ob das Layout nicht kollabiert ist
```

Plus Konvention: „Für jeden API-Client und jeden neuen Controller sind sowohl Unit-Tests mit gemocktem `HttpMessageHandler` als auch Integrationstests mit `WebApplicationFactory` bereitzustellen." — der FQN-Filter ist hier dokumentiert und müsste bei Umstellung auf Trait-Filter angepasst werden.

## `.gitignore`

- Test-Ergebnisse abgedeckt: `[Tt]est[Rr]esult*/` (Zeile ~65) → matcht `TestResults/`, `TestResults-Regular/`; `*.trx` (Zeile ~67).
- **`playwright-traces/` ist NICHT in `.gitignore`** (verifiziert via `git check-ignore` — kein Treffer). Lokale Trace-ZIPs landen als untracked Dateien im Repo-Root.
- `*.db`, `*.db-shm`, `*.db-wal` sind ignoriert (deckt `schnittstellenzentrale-playwright.db` ab).
- `issue.md`, `/.agents/*`, `/AGENTS.md` sind repo-lokal ignoriert.

## Repo-Root-Bestand (relevant)

- `issue.md` — vollständiger Issue-Text (in `.gitignore`, lokal vorhanden).
- `TestResults-Regular/` — Verzeichnis im Repo-Root mit GUID-Unterordner (ignoriert; offenbar Rest eines lokalen `dotnet test`-Laufs mit `-r TestResults-Regular`).
- `global.json` — SDK `10.0.301`, `rollForward: latestFeature`.
- `task.json` — veralteter Inhalt (`issueNumber: 48`); nicht task-relevant.
- `site/` — GitHub-Pages-Präsentationsseite; `SiteScreenshotTests` schreibt PNGs nach `site/screenshots/` (eingecheckte Dateien), `SitePageTests` testet `site/` via Route-Interception.

## Cross-Repo-Referenzen (aus Kommentaren)

- CI-Kommentare verweisen auf ein Schema-Dokument `ci-target-schema.md` (Abschnitte 3, 4.2, 4.5, 6.3, 10, 11) und Blaupausen in `FinanceManager`/`Rezepte` — das Schema-Dokument liegt **nicht** in diesem Repo.
- `requirement.md` verweist auf `martin-stromberg/Softwareschmiede` Issue #428 / PR #432 (u. a. `TempDirectoryCleanup`-Helfer, `docs/help/stabilitaet/os-interface-tests.md`) als Blaupause.
