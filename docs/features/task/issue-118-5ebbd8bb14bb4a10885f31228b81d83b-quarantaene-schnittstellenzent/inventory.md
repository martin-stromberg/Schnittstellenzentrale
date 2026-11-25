# Bestandsaufnahme – Issue #118: Quarantäne `ODataEndpointExecutionTests.OdataApi_Registrieren_Importieren_UndAlleEndpunkteErfolgreichAusfuehren`

Recherchedatum: 2026-11-25. Branch: `task/issue-118-5ebbd8bb14bb4a10885f31228b81d83b-quarantaene-schnittstellenzent`.
Anforderung: `requirement.md` (Issue #118, auto-generiert vom Flake-Analyzer).

## Übersicht der Befundlage

| Kennzahl | Wert | Quelle |
|---|---|---|
| Auslöser | Fehlraten-Schwelle (22,2 % ≥ 20 % bei 9 ≥ 5 Ausführungen), **nicht** Flake-Events | `issue.md`, `scripts/Analyze-FlakyTestResults.ps1:201-204` |
| Fehlschlagende Runs | **2**, identische Fehlersignatur | `inventory/ci-evidence.md` §1 |
| Fehlschlagende Zeile | `ODataEndpointExecutionTests.cs:47` (erste Baum-Assertion nach „Speichern") | TRX-StackTrace beider Runs |
| Fehlerbild | **Strict-mode violation**: Locator `.sz-tree-item-btn` HasText „SZ-OData-E2E" → 2 Elemente | TRX-`<Message>` beider Runs |
| Konkrete Ursache (dom-Beleg) | **Alt-Daten aus dem Geschwistertest** („SZ-OData-E2E" + „Filter-Test-App" von `OdataApi_GetEndpoints…`) waren zur Laufzeit im Baum → neuer gleichnamiger Eintrag ergab Duplikat | Trace-Snapshots, `inventory/ci-evidence.md` §4 |
| Verdacht aus dem Issue (Chevron-Blindklicks à la #112) | **Nicht** das beobachtete Fehlerbild — Test scheiterte vor jedem Chevron-Klick | `inventory/ci-evidence.md` §6 |
| `[Flaky]`-Status | Test ist **nicht** markiert (Klasse trägt nur `[OsInterface]`) | `ODataEndpointExecutionTests.cs:15-17` |

## 1. Die Testdatei `ODataEndpointExecutionTests.cs`

`src/Schnittstellenzentrale.Tests/Playwright/ODataEndpointExecutionTests.cs` (219 Zeilen).

- `[Collection("PlaywrightODataEndpoint")]` + `[OsInterface]` auf Klassenebene (Z. 15-16);
  Fixture `PlaywrightODataEndpointServer` via Ctor (Z. 19-27).
- Klasse enthält **2 Facts**; beide registrieren eine Anwendung mit **demselben Namen
  `SZ-OData-E2E`** und derselben OData-Basis-URL `{BaseUrl}/odatav4`:
  - `OdataApi_Registrieren_Importieren_UndAlleEndpunkteErfolgreichAusfuehren` (Z. 34-116, ~30 UI-Interaktionen):
    1. `GotoAsync` + `NetworkIdle` + Workspaces-Tab (Z. 37-39)
    2. „+ Neue Anwendung" → Felder Name/Basis-URL/Schnittstellen-URL → „Speichern" (Z. 42-46)
    3. **Z. 47**: `Expect(.sz-tree-item-btn HasText "SZ-OData-E2E").ToBeVisibleAsync()` — ungefiltert/strict-anfällig, **genau hier scheiterten beide CI-Fehlschläge**
    4. Klick `.sz-tree-item-btn` „SZ-OData-E2E" `.First` → „OData-Import" → Vorschau-Wait → „Übernehmen" (Z. 50-53)
    5. App-Chevron zweimal blind geklickt (Z. 56-60), ebenso Z. 78-79 und 98-99 — das bekannte Doppelklick-Muster (siehe §5)
    6. `Authenticate`-Endpunkt ausführen: Wait (Z. 63-64), Klick, `.sz-endpoint-name-input`-Value-Check, „Anfrage senden" (Z. 65-67); Status-Assertion 2xx via `.response-section strong` (Z. 69-74), Body enthält `token` (`.sz-response-body-pre`, Z. 75)
    7. Gruppen-Chevron „ApplicationGroups" (Z. 81-84) → „GET ApplicationGroups" ausführen, Status 2xx + Body `isSystem` (Z. 86-95)
    8. Gruppen-Chevron „Applications" (Z. 101-104) → „GET Applications" ausführen, Status 2xx + `isSystem` (Z. 106-115)
  - `OdataApi_GetEndpoints_MitContainsFilter_GibtNurPassendeEndpunkteZurueck` (Z. 122-198):
    - gleicher Registrierungs-/Import-Block (Z. 130-140, inkl. ungefilterter Z. 135-Expect — dieselbe Strict-Mode-Anfälligkeit)
    - legt zusätzlich Anwendung **„Filter-Test-App"** (Z. 143-147) und seedet 3 Endpunkte **direkt per Repository** (`SeedFilterEndpointsAsync`, Z. 200-218; `IApplicationRepository`/`IEndpointRepository` aus `_server.Services`)
    - Authenticate ausführen (für Bearer-Token im Environment), dann `GET Endpoints` mit `$filter=contains(Name,'Endpunkt')` über Query-Parameter-Panel (Z. 176-183); Assertions: 2xx, Body enthält `Test-Endpunkt-Alpha`+`-Beta`, nicht `Andere-Schnittstelle`

Reihenfolge der Facts ist **nicht fixiert** (kein Orderer im Projekt; TRX-`startTime` variiert pro Lauf — `inventory/ci-evidence.md` §5).

## 2. Test-Infrastruktur

### `Playwright/Infrastructure/`

| Datei | Kernfakten |
|---|---|
| `PlaywrightTestBase.cs` | `IAsyncLifetime`; `InitializeAsync` (Z. 64-92): `TestDatabaseSeeder.ResetAsync()` **vor** Browser-Start; `BaseUrl` aus Server; Chromium `Headless=TestPaths.IsHeadless()` (CI=true); `Locale=de-DE`; Timeouts `SetDefaultTimeout(30_000)`/`SetDefaultNavigationTimeout(60_000)` (Z. 17-22, 125-129); Tracing `Screenshots+Snapshots+Sources` pro Kontext (Z. 82-87). `DisposeAsync` best-effort mit Fehlersammlung (Z. 136-166); `StopTraceWithBudgetAsync` 30 s (Z. 13, 193-225); Trace-Dateien `playwright-traces/{TestName}-{HHmmssfff}.zip` absolut am Repo-Root (Z. 28-29, 232-233) |
| `PlaywrightServer.cs` | Echter Kestrel auf `http://127.0.0.1:5099` (Z. 39); Shared-Cache-In-Memory-SQLite `Data Source={guid};Mode=Memory;Cache=Shared` + offene Anchor-Connection (Z. 69-72); `PlaywrightApiFactory` (WebApplicationFactory, In-Process-Handler statt TCP für API-Calls des Blazor-Servers, Z. 77-78, 114-115); Test-Auth via `TestAuthHandler`; `ISwaggerImportService` gemockt (Z. 119-127); Init-Phasen-Marker + `WaitForServerReadyAsync` (500 ms Poll, 60 s Deadline, jede HTTP-Antwort = ready, `UseProxy=false`, Z. 169-206); Swagger-Warmup (Z. 156-157); Dispose best-effort (Z. 226-249) |
| `PlaywrightODataEndpointServer.cs` | Port **5103** (Z. 20); ersetzt `ICredentialService` durch Mock mit festem Bearer-Token `playwright-odata-bearer-token` (Z. 17-31); API-Seite akzeptiert jedes Token via `PermissiveTokenStore` (`PlaywrightApiFactory.cs`, RemoveAll`<ITokenStore>`) |
| `PlaywrightODataEndpointCollection.cs` | `CollectionDefinition("PlaywrightODataEndpoint")`, `ICollectionFixture<PlaywrightODataEndpointServer>` — Collection-Fixture lebt über **alle** Facts der Klasse; selbe In-Memory-DB für beide Tests |
| `TestDatabaseSeeder.cs` | `ResetAsync` (Z. 30-43): `EnsureDeletedAsync()` + `EnsureCreatedAsync()` + `SystemEntryInitializer.InitializeAsync` + `SeedSystemEndpointsAsync` (legt `POST /authenticate` an). **Dokumentierte Unzuverlässigkeit:** Kommentar zu `SeedShowcaseDataAsync` (Z. 44-48): „Auf der benannten Shared-Cache-In-Memory-SQLite der Playwright-Server wirkt `EnsureDeletedAsync` nicht zuverlässig, sodass sich Daten über die Facts einer Klasse sonst akkumulieren würden." `SeedShowcaseDataAsync` umgeht das manuell via `ClearNonSystemDataAsync` (Z. 64-75: löscht Nicht-System-Gruppen kaskadierend, ungruppierte Nicht-System-Apps, Umgebungen) |
| `TestPaths.cs` | `FindRepoRoot()` sucht `Schnittstellenzentrale.slnx` aufwärts (Z. 26-37); `IsHeadless()` = `CI=true` (Z. 14-18) |
| `PlaywrightApiFactory.cs` | `internal sealed`, WebApplicationFactory<Program>; gleiche Shared-DB; `PermissiveTokenStore` (innerhalb der Datei); TestAuth; gemockter SwaggerImportService |
| Weitere Server/Ports | `PlaywrightSignalRServer` :5100, `PlaywrightImpressumServer` :5101, `PlaywrightImpressumLanguageServer` :5102, `PlaywrightImpressumFallbackServer` :5104 — jeweils eigene Collection-Definition |

### `Infrastructure/Testing/`

| Datei | Kernfakten |
|---|---|
| `OsInterfaceAttribute.cs` | `Attribute` + `Xunit.v3.ITraitAttribute`, liefert `("Category","OsInterface")` über `GetTraits()`; Klasse oder Methode; Kommentar nennt Ports 5099-5104 und best-effort-Lane |
| `FlakyAttribute.cs` | Trait `Category=Flaky`; Vergaberegeln im XML-Doc: nur mit `OsInterface`, nur bei dokumentiertem Flake/Analyzer-Befund, entfernen nach Fix |
| `OsInterfaceFactAttribute.cs` / `OsInterfaceTheoryAttribute.cs` | Fact-/Theory-Varianten für Einzelmethoden (CallerFilePath/LineNumber an Basis weitergegeben); Theory-Variante aktuell ohne Anwender |
| `TestCategories.cs` | Konstanten `OsInterface`, `Flaky` |
| `FlakyConventionTests.cs` | Reguläre Lane; Assembly-Scan: `Category=Flaky` ⇒ muss `Category=OsInterface` haben (Klassen-Traits vererben auf Methoden, Z. 15-35) |
| `TraitParityTests.cs` | Reguläre Lane; jeder Typ mit „Playwright" im `FullName` + ≥1 Fact muss `OsInterface` tragen (Z. 20-55) — Parität zum früheren `FullyQualifiedName~Playwright`-Filter |
| `TestTraitReader.cs` | Hilfsleser für Traits (von beiden Meta-Tests genutzt) |

## 3. Härtungsmuster `ExpandWithRetryAsync` (Issue #112)

`src/Schnittstellenzentrale.Tests/Playwright/TreeCollapseTests.cs`:

- **`ExpandWithRetryAsync(ILocator waitTarget, ILocator clickTarget)`** (Z. 143-160, `private static async Task`): bis zu 3 Versuche — `waitTarget.WaitForAsync(Timeout=5_000)`; bei `TimeoutException` → `clickTarget.ClickAsync()`; danach finaler `waitTarget.WaitForAsync()` mit Default-Timeout.
- **Aufrufstellen:** `ExpandSystemGroupAsync` (Z. 116-122, waitTarget=`SystemAppRow .sz-tree-item-btn`, clickTarget=`.collapsible-section .sz-tree-chevron-btn`.First) und `ExpandSystemApplicationAsync` (Z. 128-134, waitTarget=`SystemAppRow .sz-tree-chevron-btn .sz-icon-chevron-down`, clickTarget=`SystemAppRow .sz-tree-item-btn`); eingesetzt in `ClickEndpointGroupName_TogglesCollapse` (Z. 171, 181), `ClickEndpointGroupChevron_TogglesCollapse` (Z. 199, 209), `EndpointGroupInitiallyCollapsed` (Z. 228, 252-253).
- **Kommentar** (Z. 136-142): zwei abgefederte Flake-Ursachen — verlorener Klick während SignalR-Reconnect nach `Page.ReloadAsync()`; Toggle-Semantik (Blindklick klappt bereits Expandieres wieder zu). Sichtbarkeitsprüfung **vor** jedem Klick.
- **Dokumentiert in** `docs/help/stabilitaet/os-interface-tests.md:152` („Verifizierter Expand-Klick statt Blindklick") als CI-bewiesener Mechanismus aus Issue #112.

### Issue-#112-Kontext im Repo

- `docs/features/task/issue-112-1f612fe42f2340a5897c6a7e08820566-quarantaene-schnittstellenzent-tasks.md` (Task-Reste): Befund dort = Run **45076613896**, 30-s-Timeout auf `SystemAppRow .sz-tree-item-btn` an `appBtn.ClickAsync()`, verlorener Gruppen-Chevron-Klick nach `ReloadAsync`, Code-Stand `0095a17`, Trace `TreeCollapseTests-221833010.zip`.
- `docs/help/stabilitaet/os-interface-tests.md:77`: `TreeCollapseTests.EndpointGroupInitiallyCollapsed` ist der **einzige** aktuell `[Flaky]`-markierte Test (`TreeCollapseTests.cs:222`); Markierung = Analyzer-Befund 1/5 Fehlschläge = 20 %.
- Git-History: `462d0c3 fix: Quarantaene-Haertung TreeCollapseTests.EndpointGroupInitiallyCollapsed (#117)`, `ce5f00b`, `3f7da5d` (Plan-Commit).
- **Abgrenzung:** Der #112-Mechanismus (Timeout beim verlorenen Klick) ist **nicht** das hier beobachtete Fehlerbild (Strict-Mode-Verstoß vor jedem Chevron-Klick) — `inventory/ci-evidence.md` §6.

## 4. `sz-tree-*`-Selektoren und Chevron-Mechanik

| Selektor | Quelle | Semantik |
|---|---|---|
| `.sz-tree-row` | `ApplicationGroupTree.razor:461,477,499,528`; `CollapsibleSection.razor:6` | Zeilen-Container (App-Zeile `draggable` außer `IsSystem`) |
| `.sz-tree-chevron-btn` | `CollapsibleSection.razor:7` (`@onclick="Toggle"` → `_expanded=!_expanded`, rein lokal); `ApplicationGroupTree.razor:462` (`ToggleEndpointGroupExpanded`, Z. 286-292 HashSet-Toggle) und `:500` (`ToggleApplicationExpanded`, Z. 263-278 — async: Expand lädt `ReloadApplicationDataAsync` + SignalR `SubscribeToApplication`, Collapse → `UnsubscribeFromApplication`) | **Toggle-Semantik**: jeder Klick invertiert; Chevron-Icon `sz-icon-chevron-down`/`sz-icon-chevron-right` spiegelt den Zustand |
| `.sz-tree-item-btn` | `ApplicationGroupTree.razor:504` (`SelectAndExpandApplication` = ToggleApplicationExpanded + `SelectApplication`) und `:480,531` (`RequestSelectEndpoint`) | Anwendungsname = Klick togglet Expand + selektiert; Endpunktname = selektiert |
| `.sz-tree-item-label` | `ApplicationGroupTree.razor:466` (`RequestSelectEndpointGroup` → ToggleEndpointGroupExpanded + `OnEndpointGroupSelected`, Z. 329-333) | Ordnername = togglet + selektiert |
| `.sz-tree-node-text` | `CollapsibleSection.razor:14` (`OnTitleClick` → `_expanded=!_expanded` + `OnTitleClicked`-Callback) | Gruppen-/Sektionstitel togglet ebenfalls |
| `.sz-icon-chevron-down`/`-right` | `CollapsibleSection.razor:8`, `ApplicationGroupTree.razor:463,501`; CSS `app.css:1742-1743` | Icon = Expand-Zustand |
| „Ohne Sammlung"-Sektion | `ApplicationGroupTree.razor:47`: `CollapsibleSection` mit `InitiallyExpanded="true"` für `_ungroupedApplications` — ungruppierte Apps sind **immer sichtbar** ohne Expand-Klick |

Wichtig für den Befund: Eine neue ungruppierte Anwendung erscheint sofort als
`.sz-tree-item-btn` in der „Ohne Sammlung"-Sektion — bei einem Alt-Daten-Duplikat
zweimal identisch → Strict-Mode-Verstoß an `ODataEndpointExecutionTests.cs:47`.

## 5. OData-Import und Endpunkt-Ausführung (Produktseite)

- OData-EDM: `OData/ODataEdmModelBuilder.cs` — EntitySets `Applications`, `ApplicationGroups`, `Endpoints`, `EndpointGroups` (Z. 27-39) + `Authenticate`-Bound-Action; Vocabulary-Annotations für Bearer-Auth + `EntitySetScript` (401→Authenticate→repeat, X-New-Token-Header, Z. 18, 59-67).
- `ODataImportService` (`Schnittstellenzentrale.Infrastructure/Services/ODataImportService.cs`): `ExtractEntitySetName` gruppiert importierte Endpunkte nach Entity-Set — Endpunkte wie „GET ApplicationGroups" landen in gleichnamigen `EndpointGroup`s; Operationen ohne Leerzeichen-Präfix („Authenticate") bleiben ungrouped (Kommentar Z. 223-224).
- UI-Pfade: „OData-Import"-Button in `ApplicationContentView.razor:34` → `ODataImportDialog`; `.sz-endpoint-name-input` `EndpointPage.razor:16`; „Anfrage senden" `EndpointPage.razor:42` (`SendRequestAsync`, disabled während Ausführung); Response-Block `.sz-endpoint-response.response-section` ab Z. 99.
- Anwendungs-Anlage: `ApplicationEditor.razor` — `EditForm`/`OnValidSubmit="SaveAsync"` (Z. 29, 134-159), `AddApplicationAsync` → `OnSaved` → `WorkspacesLayout.OnApplicationSaved` (`WorkspacesLayout.razor:174-181`: `RefreshTreeAsync` + `SelectApplicationAsync`). Kein `_isSaving`-Guard; ein Klick = ein Submit-Event.
- API: `ApplicationsController` POST `/api/applications` (Z. 117-134), `GET /api/applications/ungrouped` (Z. 67-77); `ApplicationRepository.GetUngroupedApplicationsAsync` = `Where(ApplicationGroupId == null)` (Z. 139-150); `NotifyTreeMutationAsync` nach jeder Mutation (SignalR „TreeChanged" → `ApplicationGroupTree` `LoadDataAsync`, `ApplicationGroupTree.razor:135-139`).

## 6. Skripte

| Datei | Kernfakten |
|---|---|
| `scripts/Analyze-FlakyTestResults.ps1` | Lauf-Gruppierung `<name>.trx`/`<name>-flaky-retry.trx` (Z. 99-104); Fenster `MaxRuns=20` Primär-Läufe, Sortierung über `<Times start>` mit `LastWriteTimeUtc`-Fallback (Z. 109-112); Flake-Ereignis = fail→grün im selben-Lauf-Retry ODER fail→grün Folge-Primärlauf (Z. 172-190); `NotExecuted`/Discovery-Ausfälle = fehlende Beobachtung (Z. 153); Quarantäne = ≥2 Flake-Events **ODER** Fehlrate ≥0,2 bei ≥5 Ausführungen (Z. 77-80, 201-204); `beobachten` ≥1 Event oder ≥10 %; schreibt Kandidaten-JSON (Z. 241-253); Exit immer 0 |
| `scripts/Retry-FlakyTests.ps1` | `if: failure()`-Schritt; parst Primär-TRX, schneidet mit `Category=Flaky`-Menge (`lib/TestDiscovery.ps1 Get-FlakyTestNames` via `dotnet test --no-build --list-tests -- --filter`); genau 1 Re-Run `--filter "(Category=Flaky)&(FullyQualifiedName~…)"` → `<name>-flaky-retry.trx` (Z. 105-117); Exit 1 bei persistentem Fehlschlag. **Für den hier betrachteten Test ein No-op** (nicht Flaky-markiert — belegt: kein `*-flaky-retry.trx` in den Artefakten) |
| `scripts/New-FlakyQuarantineIssue.ps1` | Dedupe über `gh issue list --search "in:title …" --state open` (Z. 75); Issue-Body mit Marker `<!-- flaky-quarantine:<fqn> -->` + Kennzahlen-Tabelle (Z. 95-119); best-effort (Fork-PRs: `issues: write` wirkungslos → Warnung + Exit 0) |
| `scripts/Download-OsInterfaceTrxHistory.ps1` | `gh run list --workflow <file> --limit MaxRuns(15) --status completed` → `gh run download -n <ArtifactName>` pro Lauf nach `trx-history/<run-id>/`; **nur gleiches Workflow-File** (Kommentar Z. 21-22); best-effort Exit 0 |
| `scripts/lib/Trx.ps1` | `Get-TrxTestResults` (testName→outcome via `//t:UnitTestResult`), `Get-TrxRunStartTime` (`//t:Times/@start`) — Namespace `http://microsoft.com/schemas/VisualStudio/TeamTest/2010` |
| `scripts/lib/TestDiscovery.ps1` | `Get-FlakyTestNames` → HashSet der `Category=Flaky`-Methoden-FQNs |

## 7. CI

### `.github/workflows/pr-staging-ci.yml` (Name „PR CI for Staging")

- Job `os-interface-tests` (Z. 199-310): `continue-on-error: true` (Z. 210), `windows-latest`, `timeout-minutes: 45`; eigene Permissions `contents:read`+`issues:write`+`actions:read`, `GH_TOKEN` (Z. 216-221).
- Steps: Build → **„Test OS interface"** `dotnet test --solution … -c Release --no-build --results-directory src/Schnittstellenzentrale.Tests/TestResults -- --filter "Category=OsInterface" --report-trx --report-trx-filename test-results-os-interface.trx` (Z. 244) → „Retry flaky tests once" `if: failure()` (Z. 247-258) → „Download OS-interface TRX history" `if: always()`+`continue-on-error` mit `-WorkflowFile pr-staging-ci.yml -ArtifactName test-results-os-interface-pr -MaxRuns 15` (Z. 260-269) → „Analyze flaky test results" `if: always()` über `TestResults, trx-history` → `flaky-quarantine-candidates.json` (Z. 271-278) → „Create quarantine issues" `if: always()` (Z. 280-287) → Upload `test-results-os-interface-pr` (`test-results-os-interface*.trx`, 14 d, Z. 289-296) → Upload `playwright-traces-pr` (`playwright-traces/`, 14 d, Z. 298-310).
- `detect-backmerge` lässt Backmerge-PRs alle Jobs überspringen (deshalb `skipped`-Jobs in der Historie).

### `.github/workflows/staging-ci.yml` (Name „Pre-Release")

- Job `os-interface-tests` (Z. 176-286): identischer Aufbau; Artefakte `test-results-os-interface-staging` / `playwright-traces-staging`; History-Download mit `-WorkflowFile staging-ci.yml` (Z. 236-245).
- `version`/`prerelease` `needs:` enthalten den OS-Job **nicht** (Z. 292) — OS-Fehlschlag blockiert nie die Release-Kette.

## 8. Dokumentation

- `docs/help/stabilitaet/index.md`: Index, verlinkt nur `os-interface-tests.md`.
- `docs/help/stabilitaet/os-interface-tests.md` (190 Zeilen), relevante Abschnitte:
  - `## Klassifizierungskriterium` (Z. 7-28): OS-Schnittstellen-Heuristik; „Aktuell getaggte Klassen: alle 24 Testklassen unter `…Tests.Playwright`" (Z. 28).
  - `## Flaky-Kategorie` (Z. 50-77): Vergaberegeln; Z. 77: genau **ein** `[Flaky]`-Test (`EndpointGroupInitiallyCollapsed`).
  - `## Lokale Befehle` (Z. 79-92): xunit.v3/MTP-Konventionen (s. §9).
  - `## Automatischer Retry (Flaky)` (Z. 94-113), `## Flake-Statistik und Quarantäne` (Z. 115-134), `## CI-Verhalten` (Z. 136-145).
  - `## Infrastruktur-Härtung` (Z. 147-153): dokumentierte Muster inkl. `ExpandWithRetryAsync` (#112) und Dirty-Badge-vor-Reload.
  - `## Bekannte Fehlerbilder` (Z. 155-161): Port belegt, Blazor-Erstaufruf >30 s, `Tracing.StopAsync`-Hang, Datei-Lösch-Locks, Trace-Diagnose. **Das hier beobachtete Fehlerbild (Alt-Daten-Duplikat → Strict-Mode-Verstoß) ist dort noch nicht erfasst.**
  - `## Env-Flags und Retry` (Z. 163-169): keine Skip-Flags; Job-Rerun-Konvention `gh run rerun --job <id>`.
  - `## Playwright-Versionsupdates` (Z. 171-173): lokales OsInterface-Gate vor Merge.
  - `## Audit-Heuristik und Meta-Tests` (Z. 175-190).

## 9. xunit.v3 / MTP-Konventionen

- `global.json`: `sdk.version=11.0.100`, `test.runner="Microsoft.Testing.Platform"` — `dotnet test` läuft im MTP-Modus; Filter/Reports als Test-App-Argumente **hinter `--`** (`Category=…`, `FullyQualifiedName~…`, Compound `&`/`|`).
- Leere Treffermenge → MTP-Exitcode 8 (tolerierbar via `--ignore-exit-code 8`); TRX via `Microsoft.Testing.Extensions.TrxReport` (`--report-trx`, `--report-trx-filename`); Coverage via `--coverage --coverage-output-format cobertura` (`docs/help/stabilitaet/os-interface-tests.md:81`).
- `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj`: `xunit.v3 4.0.3`, `Microsoft.Playwright 1.65.1`, `Microsoft.EntityFrameworkCore.Sqlite 11.0.*`; Playwright-Chromium-Install via `InstallPlaywright`-MSBuild-Target (`AfterTargets="Build"`, abschaltbar `-p:SkipPlaywrightInstall=true`, laut CI-Kommentar `pr-staging-ci.yml:133-137` / Doku Z. 173).
- Kein Test-Orderer konfiguriert (`grep Orderer|parallelize` → keine Treffer); Fact-Reihenfolge innerhalb der Collection variiert beobachtbar pro Lauf (`inventory/ci-evidence.md` §5).

## 10. Ähnliche Flows in anderen Playwright-Tests

| Datei | Überdeckung |
|---|---|
| `Playwright/ODataImportTests.cs` | Registriert OData-Apps (`OData-Test-Anwendung`, `OData-CRUD-Anwendung`), OData-Import-Dialog, Baum-Expand — **dasselbe blinde Chevron-Doppelklick-Muster** (Z. 52-53, 86-87, 107/112). Collection `Playwright` (Port 5099). Keine Endpunkt-Ausführung mit Bearer-Token. |
| `Playwright/EndpointExecutionTests.cs` | Endpunkt-Ausführung via UI: `ExecuteEndpoint_ReturnsSuccessResponse`, `AuthenticateEndpunkt_Auswaehlen_GibtTokenZurueck` (System-App `POST /authenticate`), Platzhalter-/Query-String-Flows. Collection `Playwright`. Nutzt eigene `ExpandSystemGroupAsync` (Z. 17-21) — einfacher Blindklick, **ohne** Retry-Muster. |
| `Playwright/SiteScreenshotTests.cs` | enthält `ClickUntilVisibleAsync`-Retry-Muster für Baum-Expand (Z. 68, 114) |
| `Playwright/SwaggerImportTests.cs` | Chevron-Klicks + `.First` (Z. 24-37), REST-Import-Flow |
| `Playwright/TreeCollapseTests.cs` | `ExpandWithRetryAsync`-Referenzimplementierung (s. §3) |

Kein anderer Test deckt den OData-Import→Ausführen-Pfad mit Bearer-Token ab —
`PlaywrightODataEndpointCollection`/`Port 5103` existiert ausschließlich für
`ODataEndpointExecutionTests`.

## 11. CI-Artefakte / Trace-Mechanik

- `PlaywrightTestBase.DisposeAsync` → `StopTraceWithBudgetAsync` → `playwright-traces/{Klassenname}-{UTC HHmmssfff}.zip`, absolut verankert via `TestPaths.FindRepoRoot()` (`PlaywrightTestBase.cs:28-29,232-233`) → CI-Glob `playwright-traces/` matcht immer (`pr-staging-ci.yml:308-309`).
- TRX-Upload: `test-results-os-interface*.trx` (Primär + evtl. `-flaky-retry`) → Artefakt `test-results-os-interface-pr`/`-staging`, Retention 14 d.
- Historie-Persistenz der Flake-Statistik ausschließlich über die Artefakt-Kette (`Download-OsInterfaceTrxHistory.ps1`), kein committed State.
- Trace-Inspektion lokal: `npx playwright show-trace <zip>`; die Trace-ZIPs enthalten `trace.trace` (Actions/Logs), `trace.network`, `frame-snapshot`-Events mit Inline-DOM, `resources/` (HTML-Basis-Snapshots, CSS, Blazor-WebSocket-`.jsonl`), `screencast/*.jpeg`.

## 12. GitHub-Befund (Kurzfassung — Details: `inventory/ci-evidence.md`)

- **2 fehlschlagende Runs**, beide `pr-staging-ci.yml`, beide identische Fehlersignatur:
  - Run [45434716126](https://github.com/martin-stromberg/Schnittstellenzentrale/actions/runs/45434716126) — 2026-11-24, PR-Branch `task/issue-107-…` (NuGet-Update), Job `os-interface tests` failed.
  - Run [45543708029](https://github.com/martin-stromberg/Schnittstellenzentrale/actions/runs/45543708029) — 2026-11-25, PR-Branch `task/issue-112-…` (PR #117), Job failed; erzeugte Issue #118 um 04:05:36Z.
- Fehler: `strict mode violation` — `.sz-tree-item-btn` HasText `SZ-OData-E2E` → 2 Elemente, Zeile 47, Sofort-Fehlschlag (kein Timeout).
- Trace-DOM-Beleg: Alt-App `SZ-OData-E2E` **und** `Filter-Test-App` (nur vom Geschwistertest angelegt) im Baum des fehlschlagenden Tests → Daten der Collection-Fixture wurden zwischen den Facts nicht zurückgesetzt (`EnsureDeletedAsync` auf Shared-Cache-In-Memory-SQLite — im Repo bereits als „nicht zuverlässig" dokumentiert, `TestDatabaseSeeder.cs:44-48`).
- In 4 weiteren Läufen derselben Ausführungsreihenfolge (GetEndpoints → Registrieren) bestand der Test — trotz ebenfalls vorhandener Alt-Daten (Reset = deterministischer No-op bei `Mode=Memory`, s. ci-evidence §5): Die Intermittenz ist eine DOM-/Render-Race an Zeile 47, kein Reset-Race.
- `staging-ci.yml`: alle beobachteten Läufe grün (inkl. Run 45550953861, ~1 h nach Issue-Erstellung).

## 13. Anforderungsrelevante Zusatzfakten

- „Bekannte Fehlerbilder"-Eintrag nötig, falls das Fehlerbild als neu klassifiziert wird: Alt-Daten-/Strict-Mode-Duplikat ist aktuell **nicht** in `os-interface-tests.md:155-161` gelistet (Akzeptanzkriterium 5 der Anforderung).
- Issue #118 enthält einen ausführlichen Analyse-Kommentar (`softwarewerkstatt`, 2026-11-25T08:46Z): benennt u. a. fehlende Run-Links, den Verdacht Chevron-Doppelklicks (#112-Muster) und die Lesart „0 Flake-Events = keine Recovery beobachtet → persistente Störung wahrscheinlich". Der Chevron-Verdacht ist durch den Trace-Befund widerlegt (Fehlschlag vor jedem Chevron-Klick); die Persistenz-Lesart wird durch den Alt-Daten-Befund gestützt (Testisolations-Defekt, nicht Produktdefekt).
- `issue.md` spiegelt den Auto-Issue-Text inkl. Kennzahlen und `<!-- flaky-quarantine:… -->`-Marker (Z. 14).
