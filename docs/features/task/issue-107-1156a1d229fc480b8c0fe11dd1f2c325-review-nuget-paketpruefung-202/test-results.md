# Test-Ergebnisse

**Datum:** 2026-11-23 · **Branch:** `task/issue-107-1156a1d229fc480b8c0fe11dd1f2c325-review-nuget-paketpruefung-202` · **SDK:** .NET 10.0.404

## Ergebnis

**Status:** Keine Fehler

## Ausgeführte Prüfungen

| # | Prüfung | Befund |
|---|---------|--------|
| 1 | `dotnet build Schnittstellenzentrale.slnx -c Release -p:TreatWarningsAsErrors=true` | Erfolgreich — **0 Warnungen, 0 Fehler** (alle 4 Projekte, `net10.0`) |
| 2 | `dotnet test Schnittstellenzentrale.slnx -c Release --filter "Category!=OsInterface" --logger "console;verbosity=normal"` | **641/641 bestanden**, 0 Fehler, 0 übersprungen (Dauer 19,05 s). Entspricht der Baseline aus `inventory/test-results/dotnet-test-baseline.log` (641/641). |
| 3 | `dotnet list <projekt> package --vulnerable --include-transitive` (alle 4 Projekte: Schnittstellenzentrale, Core, Infrastructure, Tests) | **0 Befunde** — „liegen gemäß den aktuellen Quellen keine anfälligen Pakete vor" (je Projekt) |
| 4 | Reale End-to-End-Abnahme (s. Abschnitt unten) | Bestanden — App real gestartet, HTTP 200, sauber beendet |
| 5 | `dotnet format Schnittstellenzentrale.slnx --verify-no-changes --no-restore --severity error` (CI-Gate `format` aus `pr-staging-ci.yml`) | **Exit 0**, keine Verstöße |

## E2E-Abdeckung

Keine geplanten E2E-Tests vorhanden — laut `plan.md` (Abschnitt „E2E-Tests") und `plan-check.md` (Abschnitt „E2E-Abdeckung") sind explizit **keine** neuen E2E-Tests erforderlich: Die Anforderung ändert ausschließlich Paketversionen auf Build-Ebene, kein Benutzerfluss. Der einzige UI-nah berührte Punkt (`ShadcnBlazor` 1.0.14 → 1.0.22) ist durch die bestehenden `LayoutSmokeTests` (BoundingBox-Assertions) in der regulären Lane abgesichert und im 641/641-Lauf enthalten.

| Szenario | Test / Testklasse | Ergebnis |
|----------|-------------------|----------|
| Layout-Integrität nach ShadcnBlazor-Patch | `LayoutSmokeTests` (reguläre Lane, in den 641 enthalten) | Bestanden |

## Reale End-to-End-Abnahme

Begründung der Erforderlichkeit: Die Anforderung berührt Persistenz/externe Dienste indirekt über Paketversionen (EF Core, Jint, SignalR usw.) — reale Abnahme per Lifecycle-Kommando Schritt 4 daher als realer Anwendungsstart durchgeführt.

**Durchführung:** Gebautes Release-Binary `src/Schnittstellenzentrale/bin/Release/net10.0/Schnittstellenzentrale.exe` real gestartet mit `ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://127.0.0.1:5090` (kein Test-Framework, kein Mock). Anschließend per Prozessabbruch sauber beendet; `curl` danach → Verbindung verweigert (App weg).

| Nachweis | Ergebnis |
|----------|----------|
| `GET http://127.0.0.1:5090/` | **HTTP 200** — echtes HTML (`<base href="/">`, `shadcn/default-theme.css`, `Schnittstellenzentrale.*.css`, Blazor-Markup) |
| `GET /swagger` → `301` → `GET /swagger/index.html` | **HTTP 200** (Swagger UI ausgeliefert) |
| `POST /hubs/endpoint/negotiate` (SignalR, aus App-Log) | **HTTP 200** `application/json` |
| App-Log (`bin/Release/net10.0/logs/log-20261123.txt`) | **0 `[ERR]`/`[FTL]`-Einträge.** Vorhandene `[WRN]`/`[INF]`: EF-Migrationshinweise (SQLite-Tabellen-Rebuild, nicht-transaktionale Operationen — bekannt, migrationsbedingt), „Failed to determine the https port for redirect" (kein HTTPS konfiguriert — erwartet), 2× `HttpRequestException localhost:7295` `[INF]` — der interne `IApplicationApiClient` zeigt per `appsettings.Development.json` auf `https://localhost:7295`, das in dieser Einzelinstanz nicht läuft; Konfigurationseffekt, kein Laufzeitfehler (Seite wird trotzdem mit 200 geliefert). |
| Beendigung | Prozess beendet; Port 5090 anschließend nicht erreichbar (`curl` → 000). Temporäre `schnittstellenzentrale.db` (SQLite, im bin/-Verzeichnis, gitignored) entfernt. |

### Akzeptanzszenarien aus plan.md (elementweise)

| # | Szenario | Ergebnis |
|---|----------|----------|
| 1 | Happy Path: niedrig/mittel-Updates umgesetzt; `dotnet test` 641/641, Vulnerability-Scan 0, CI-Gates | **Bestanden (lokale Verifikationsteile):** Diff verifiziert — 10 Paket-Bumps + AngleSharp-Kommentar-Update in 3 `.csproj`; `dotnet test` 641/641; `--vulnerable --include-transitive` 0 Befunde; Release-TWAE-Build 0 Warnungen; Format-Gate sauber. „CI-Gates grün" + „PR enthält Änderungen/Body" sind CI-/PR-seitig und außerhalb dieses lokalen Laufs. |
| 2 | Ein Minor-Update bricht Build oder Tests → Bisektion + Umstufung | **Nicht eingetreten** (konditionales Szenario): Alle umgesetzten Updates ließen die Suite bei 641/641 — keine Bisektion nötig. |
| 3 | Update zieht neue transitive Vulnerability | **Nicht eingetreten** (konditionales Szenario): Scan zeigt weiterhin 0 Befunde in allen 4 Projekten. |
| 4 | Recherche zeigt Breaking Changes bei „mittel"-Kandidat → auf „hoch" umstufen | **Bestanden (Szenario eingetreten und korrekt behandelt):** `Microsoft.Playwright` 1.52.0→1.65.1 wurde per Release-Notes-Recherche auf „hoch" umgestuft (Breaking Changes: Locator-Warteverhalten, RFC-6265-Cookies, Aria-Snapshots, `ParamsString`, Proxy-Credentials) und **nicht** umgesetzt — Verbleib dokumentiert in `recommendation-report.md` („Aufgeschoben / hoch"). Csproj-Diff bestätigt: `Microsoft.Playwright` bleibt `1.52.0`. |

## Sichtprüfung der Abnahme-Screenshots

**Status:** Nicht erforderlich

Begründung: Reine NuGet-Paketaktualisierung auf Build-Ebene — **kein UI geändert** (Festlegung in `requirement.md`/`plan.md`: „Keine Datenmodell-, Service- oder UI-Änderungen"). Es existieren keine UI-Szenarien; es wurden keine Abnahme-Screenshots erzeugt und folglich keine geprüft. Zulässig, da die reale Abnahme kein UI-Szenario umfasste; die Layout-Integrität ist über `LayoutSmokeTests` automatisiert abgesichert (s. E2E-Abdeckung).

## Zusammenfassung

- Gesamt: 641
- Bestanden: 641
- Fehlgeschlagen: 0
- Übersprungen: 0

## Testabdeckung

**Abdeckung:** 73,87 % Zeilenabdeckung gesamt (XPlat Code Coverage / coverlet, reguläre Lane `Category!=OsInterface`; Branch-Rate 57,59 %)

| Projekt | Abdeckung |
|---------|-----------|
| Schnittstellenzentrale (Web) | 61,14 % |
| Schnittstellenzentrale.Infrastructure | 80,37 % |
| Schnittstellenzentrale.Core | 88,26 % |

Gesamtwert liegt über dem CI-Gate von 70 % (`pr-staging-ci.yml`).

## Fehlende Tests

Quelle: `Coverage-Daten` (coverage.cobertura.xml, reguläre Lane). 55 Quelldateien mit 0 % Zeilenabdeckung — **Bestandszustand, nicht durch diese Änderung verursacht** (kein Code wurde geändert; die Paket-Updates verändern die Abdeckungslage nicht). Gruppiert:

- `Schnittstellenzentrale/Components/Shared/` — 29 Razor-Komponenten, 0 % (Blazor-UI ohne bunit-Testabdeckung)
- `Schnittstellenzentrale/Components/Layout/` — 5 Dateien (`AppShell.razor`, `EnvironmentsLayout.razor`, `ThemeToggle.razor`, `TopBar.razor`, `WorkspacesLayout.razor`), 0 %
- `Schnittstellenzentrale/Components/Pages/` — 2 Razor-Seiten, 0 %
- `Schnittstellenzentrale.Infrastructure/Data/SqlServerMigrations/` — 4 Migrations-Dateien (`InitialCreate`, `AddParentGroupIdToEndpointGroup` + Designer), 0 % (generiert)
- `Schnittstellenzentrale.Core/Helpers/HttpMethodBadgeHelper.cs` — 0 % Abdeckung
- `Schnittstellenzentrale.Infrastructure/Data/AppDbContextFactory.cs` — 0 % Abdeckung (Design-Time-Factory)
- `Schnittstellenzentrale.Infrastructure/Data/ProviderFilteredMigrationsAssemblies.cs` — 0 % Abdeckung
- `Schnittstellenzentrale.Infrastructure/Repositories/ApplicationRepository.cs` — 0 % Abdeckung
- `Schnittstellenzentrale.Infrastructure/Repositories/EndpointRepository.cs` — 0 % Abdeckung
- `Schnittstellenzentrale.Infrastructure/Services/ApplicationGroupService.cs` — 0 % Abdeckung
- `Schnittstellenzentrale.Infrastructure/Services/ApplicationService.cs` — 0 % Abdeckung
- `Schnittstellenzentrale.Infrastructure/Services/ImpressumService.cs` — 0 % Abdeckung
- `Schnittstellenzentrale.Infrastructure/Services/SignalRNotificationService.cs` — 0 % Abdeckung
- `Schnittstellenzentrale.Infrastructure/Services/WindowsCredentialService.cs` — 0 % Abdeckung
- `Schnittstellenzentrale/Filters/ContextHeadersOperationFilter.cs` — 0 % Abdeckung
- `Schnittstellenzentrale/Filters/SecurityOperationFilter.cs` — 0 % Abdeckung
- `Schnittstellenzentrale/Filters/SzExtensionsOperationFilter.cs` — 0 % Abdeckung
- `Schnittstellenzentrale/OData/ODataApplicationsController.cs` — 0 % Abdeckung
- `Schnittstellenzentrale/Services/ApplicationApiClient.cs` — 0 % Abdeckung

## Hinweise / Abgrenzungen

- `Microsoft.Playwright` wurde in dieser Änderung bewusst **nicht** aktualisiert (auf „hoch" umgestuft, Folge-Issue) — die OsInterface-Testlane (`Category=OsInterface`) wurde entsprechend der Task-Vorgabe lokal nicht ausgeführt; sie läuft best-effort in der CI.
- Die konditionalen Akzeptanzszenarien 2 und 3 sind Fehlerpfade; ihre Bedingungen traten nicht ein (Suite grün, Scan sauber) — sie werden nicht als fehlgeschlagene Tests gewertet.
- Lokaler Verifikationsumfang ist identisch mit den CI-Gates 1–4 (Format, Security-Scan, TWAE-Release-Build, reguläre Testlane); Coverage-Gate ≥ 70 % mit 73,87 % übererfüllt. Der finale „CI-Gates grün"-Nachweis selbst liegt beim PR-Lauf.
