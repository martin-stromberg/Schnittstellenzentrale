# Umsetzungsplan: [Review] NuGet-Paketprüfung 2026-KW48 (Issue #107)

## Übersicht

Wartungs-/Prüfaufgabe auf Build-Ebene: Die `PackageReference`-Einträge der vier `.csproj`-Dateien werden anhand der Bestandsaufnahme risikobewertet. Updates mit Risiko **niedrig** (3 Patch-Kandidaten) und **mittel** (8 Minor-Kandidaten, vorbehaltlich Release-Notes-Prüfung) werden direkt umgesetzt und lokal per Build + Testsuite verifiziert; Ergebnis ist ein PR gegen `staging` mit risikobewerteter Empfehlungstabelle im PR-Body. Hoch-Risiko-Kandidaten (Major-Sprünge, Preview-Wechsel, xunit.v3-Migration, Architekturablösungen) werden dokumentiert und in einem Folge-Issue gebündelt.

## Scope-Klärung

| Feld | Festlegung |
|------|------------|
| Verarbeitungseinheit | Eine einzelne `PackageReference` in einem der vier `.csproj` (Top-Level); transitive Pakete werden nicht direkt angefasst — sie fließen über die Top-Level-Updates und werden per Vulnerability-Scan abgesichert. |
| Auslösende Ereignisse | Issue #107 „NuGet-Paketprüfung 2026-KW48" (wöchentlich angelegt via `weekly-nuget-review.yml`). |
| Erwartete Wirkung | (1) PR gegen `staging` mit allen niedrig/mittel-Updates, der die CI-Gates besteht (Format, Security-Scan `vulnerable packages`, TreatWarningsAsErrors-Build, `dotnet test --filter "Category!=OsInterface"`, Coverage ≥ 70 %); (2) Empfehlungstabelle im PR-Body; (3) Folge-Issue für hoch/aufgeschobene Empfehlungen. |
| Umgang mit fehlenden Objekten | Empfehlungs-Report (Markdown): im Feature-Verzeichnis **erzeugen**. Folge-Issue: bei Bedarf **erzeugen** (ohne Label `automated-nuget-review`, damit die Duplikatsprüfung des Wochen-Workflows es nicht erfasst). Label `automated-nuget-review`: **wiederverwenden**, nicht neu anlegen. `packages.lock.json`/`Directory.Packages.props`: **nicht erzeugen** — Floating-`10.0.*`-Politik bleibt (Arbeitsannahme). |

### Scope-Ausschlüsse

| # | Einschränkung / Ausschluss | Begründung | Status |
|---|----------------------------|------------|--------|
| 1 | Keine Updates der `npm`-`devDependencies` (`semantic-release` u. a.) | Festlegung in `requirement.md`; nur Hinweis im Report, falls auffällig | bestätigt (requirement.md) |
| 2 | Keine Änderung der GitHub-Actions-Pins in `.github/workflows/` | Festlegung in `requirement.md`; nur Hinweis, falls auffällig | bestätigt (requirement.md) |
| 3 | Keine Umsetzung von Updates mit Risiko „hoch" (Major, Preview-Wechsel, Verhaltensänderungen) | Festlegung in `requirement.md`; nur dokumentiert + Folge-Issue | bestätigt (requirement.md) |
| 4 | Kein Wechsel der Floating-Version-Politik (`10.0.*` beibehalten, kein `packages.lock.json`) | Arbeitsannahme aus `requirement.md`; Umstellung wäre Prozessänderung über dieses Issue hinaus | bestätigt (requirement.md) |
| 5 | Kein TFM-/SDK-Upgrade (`net10.0` / SDK `10.0.301`+rollForward bleiben fix) — `11.0.0`-Updates der `Microsoft.*`-Pakete sind TFM-gebunden und damit keine Kandidaten | Festlegung in `requirement.md` | bestätigt (requirement.md) |
| 6 | Keine Änderung des wöchentlichen Review-Workflows | Festlegung in `requirement.md` | bestätigt (requirement.md) |
| 7 | Keine neuen Tests, keine Codeänderungen außerhalb der `.csproj` (Versionsangaben + Override-Kommentare) | Wartungsaufgabe; Regressionstests = bestehende Suite | bestätigt (requirement.md) |

## Designentscheidungen

| Komponente / Bereich | Gewählter Ansatz | Begründung |
|----------------------|------------------|------------|
| Floating `10.0.*`-Referenzen (13 Pakete, 17 Referenzen) | Keine `.csproj`-Änderung; in der Tabelle als „keine Aktion" ausgewiesen | Patch-Auflösung (aktuell 10.0.15) fließt implizit bei jedem `dotnet restore`; „Neueste" = 11.0.0 ist TFM-gebunden (Ausschluss 5). Nur echte Versionslinien-Updates sind Kandidaten. |
| `AngleSharp`-Direkt-Override (Tests) | Override **beibehalten** und auf `1.10.5` anheben; Begründungs-Kommentar aktualisieren | Die direkte Referenz bleibt nötig, um die Auflösung über dem transitiven Minimum aus bunit festzuhalten (nearest-wins) — die `using AngleSharp;`-Direktive in `PlaywrightServer.cs` ist ungenutzt und trägt den Override nicht. Anhebung hält Override ≥ transitivem Bedarf und auf gepatchter Linie (GHSA-pgww-w46g-26qg). Nach bunit-Update wird die tatsächliche transitive AngleSharp-Anforderung per `dotnet list package` verifiziert und der Kommentar der Faktenlage angepasst. |
| `System.Security.Cryptography.Pkcs`-Override (Infrastructure, `10.0.*`) | **Unverändert** — weder Version noch Kommentar anfassen | Override ist bereits floating und korrekt begründet (GHSA-846f-f764-76mx, GHSA-2fmv-rv2c-59x7); Parent `Microsoft.EntityFrameworkCore.SqlServer` bleibt `10.0.*`, damit bleibt die transitive Anforderung `>= 10.0.13` erhalten — kein Update des Parent, keine Neu-bewertung nötig. |
| Legacy-Pakete `Http.Abstractions` 2.3.x / `SignalR.Core` 1.2.x | **Patch-Update innerhalb der Paketlinie** (2.3.14 / 1.2.14), Ablösung durch `FrameworkReference Microsoft.AspNetCore.App` als hoch/Architekturänderung dokumentieren | `FrameworkReference` in Infrastructure würde die ASP.NET-Typen aus dem Shared Framework 10.x beziehen — ändert API-Oberfläche und Verhalten (Architekturumstellung). Patch-Updates sind niedrig und bringen die Fixstände der Legacy-Linie mit. |
| OData-Versionssplit | `Microsoft.AspNetCore.OData` 10.0.0-preview.2 **unverändert**; `Microsoft.OData.Edm` 8.4.3 **unverändert**; kohärente Strategie als hoch dokumentieren | Jede OData-Bewegung ist per Schema hoch (Preview-Wechsel bzw. Major 8→9). Empfehlung fürs Folge-Issue: `Microsoft.OData.Edm` auf `9.0.0` (stabil) angleichen — SemVer `9.0.0` > `9.0.0-preview.4` erfüllt die transitive Anforderung des AspNetCore.OData-Previews und beseitigt den Split — gebündelt mit der Entscheidung Preview→Stable/neueres Preview für `Microsoft.AspNetCore.OData`. Kein unkoordinierter Einzelsprung. |
| Update-Umsetzung | Zwei Batches: A = Patches (niedrig), B = Minoren (mittel), jeweils mit voller Verifikation; bei Testbruch Bisektion pro Paket | Minoren haben höheres Bruchrisiko (Jint-Skript-Engine, Playwright-API, Test-Plattform); Batch-Bisektion hält Attribution ohne 8 Einzelläufe im Erfolgsfall. |

## Programmabläufe

### Recherche und Risiko-Finalisierung (vor jeder `.csproj`-Änderung)

1. Pro Update-Kandidat Release Notes / NuGet-Metadaten prüfen: `Jint` 4.9.2→4.18.1 (Changelog auf Breaking Changes — Interpreter-Semantik, `EndpointScriptRunner` betroffen), `bunit` 2.7.2→2.15.2 (u. a. AngleSharp-Mindestanforderung), `Microsoft.Playwright` 1.52.0→1.65.1 (API-Brüche in der Testinfrastruktur), `Microsoft.NET.Test.Sdk` 18.5.1→18.12.0, `Moq` 4.20.72→4.21.0, `coverlet.collector` 10.0.0→10.1.1, `Markdig` 1.2.0→1.6.0, `ShadcnBlazor` 1.0.14→1.0.22, `Microsoft.AspNetCore.Http.Abstractions`→2.3.14, `Microsoft.AspNetCore.SignalR.Core`→1.2.14.
2. Zusatzrecherche (Fakten für die Dokumentation): existiert ein stabiler oder neuerer Preview-Release von `Microsoft.AspNetCore.OData` 10.x? Existiert ein `Microsoft.OData.Edm` 8.x-Patch > 8.4.3? `xunit.runner.visualstudio` 4.x-Kompatibilität mit xunit 2.9.3?
3. Risiko final einstufen: zeigt ein Kandidat Breaking Changes, wird er auf „hoch" hochgestuft und wandert in die Dokumentationsliste statt in die Umsetzung.

Beteiligte Artefakte: `inventory/outdated.md`, NuGet.org/GitHub-Release-Notes, `docs/features/task/issue-107-…/plan.md`.

### Batch A — Patch-Updates (niedrig)

1. In `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj` (Z. 24): `ShadcnBlazor` `1.0.14` → `1.0.22`.
2. In `src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj`: `Microsoft.AspNetCore.Http.Abstractions` `2.3.0` → `2.3.14` (Z. 10), `Microsoft.AspNetCore.SignalR.Core` `1.2.10` → `1.2.14` (Z. 12).
3. Verifikation: `dotnet restore Schnittstellenzentrale.slnx` → `dotnet build Schnittstellenzentrale.slnx` → `dotnet test Schnittstellenzentrale.slnx --filter "Category!=OsInterface"` (Ziel: 641/641) → `dotnet list Schnittstellenzentrale.slnx package --vulnerable --include-transitive` (Ziel: 0 Befunde).

### Batch B — Minor-Updates (mittel, vorbehaltlich Recherche)

1. `src/Schnittstellenzentrale.Infrastructure/…csproj`: `Markdig` `1.2.0` → `1.6.0` (Z. 9); `Jint` `4.9.2` → `4.18.1` (Z. 8).
2. `src/Schnittstellenzentrale.Tests/…csproj`: `Jint` `4.9.2` → `4.18.1` (Z. 23, identisch mit Infrastructure — gemeinsame Versionslinie), `bunit` `2.7.2` → `2.15.2` (Z. 18), `AngleSharp` `1.5.0` → `1.10.5` (Z. 17) **inkl. Kommentar-Update** (Z. 13–16: neue bunit-Version + ggf. neue transitive AngleSharp-Anforderung eintragen, GHSA-Verweis beibehalten), `coverlet.collector` `10.0.0` → `10.1.1` (Z. 19), `Microsoft.NET.Test.Sdk` `18.5.1` → `18.12.0` (Z. 28), `Microsoft.Playwright` `1.52.0` → `1.65.1` (Z. 29), `Moq` `4.20.72` → `4.21.0` (Z. 30).
3. `dotnet restore`, dann `dotnet list package` auf Tests-Projekt: aufgelöste transitive AngleSharp-/AngleSharp.Css-Version unter neuem bunit prüfen → Kommentar-Wahrheit sicherstellen.
4. Verifikation wie Batch A (build + test + vulnerable). Bei Testbruch: schrittweise Bisektion pro Paket — betroffenes Paket zurücksetzen, als „hoch (Verhaltensänderung/Bruch, empirisch festgestellt)" in die Dokumentationsliste überführen, Batch erneut verifizieren.

### Dokumentation und PR

1. Empfehlungstabelle erzeugen — Format aus dem Issue: Paket | aktuell → Ziel | Update-Art (Patch/Minor/Major/Security/Deprecation/Nachfolgepaket) | Risiko + 1–2-zeilige Begründung. Ablage als Markdown-Report-Artefakt `docs/features/task/issue-107-…/recommendation-report.md` (Feature-Verzeichnis; wird mit dem Lifecycle-Planungs-/Umsetzungscommit geführt bzw. beim Aufräumen entfernt — kein dauerhaftes Repo-Dokument) und gespiegelt in den PR-Body.
2. Folge-Issue anlegen (ohne Label `automated-nuget-review`): gebündelte hoch/aufgeschobene Empfehlungen — xunit→`xunit.v3`-Migration (Deprecation), `Microsoft.OpenApi` 2→3 + `Swashbuckle.AspNetCore` 10→11 (koordinierter Major-Block), `Serilog.AspNetCore` 10→11, `xunit.runner.visualstudio` 3→4, OData-Kohärenz (`Microsoft.OData.Edm` 8.4.3→9.0.0 + `Microsoft.AspNetCore.OData`-Preview-Entscheidung), `FrameworkReference`-Ablösung der Legacy-Pakete, Floating-vs-Lock-Datei-Politik, Hinweise zu npm/Actions-Pins.
3. PR gegen `staging` öffnen: Tabelle im Body, Folge-Issue verlinken, Basisbranch `staging` (Workflow Feature-Branch → staging → main).
4. PR-Checks mit Fail-fast beobachten (`gh pr checks --watch --fail-fast` bzw. Polling mit Fail-Prüfung pro Runde); bei rotem Gate sofort `gh run view --job <id> --log-failed` diagnostizieren statt weiterzuwarten.

## Neue Klassen

Keine.

## Änderungen an bestehenden Klassen

Keine — ausschließlich Versionsangaben und ein Kommentar-Block in `.csproj`-Dateien (s. Programmabläufe).

## Datenbankmigrationen

Keine.

## Validierungsregeln

Keine.

## Konfigurationsänderungen

Keine (`packages.lock.json`/`Directory.Packages.props` werden bewusst nicht eingeführt — Scope-Ausschluss 4).

## Seiteneffekte und Risiken

- **Jint 4.9.2→4.18.1:** Interpreter-Umbau über 9 Minor-Versionen; `EndpointScriptRunner` verarbeitet kundenseitige Skripte — Verhaltensänderungen möglich. Abgesichert durch `EndpointScriptRunnerTests`; bei Bruch oder Breaking Changes in den Release Notes → hoch/dokumentiert.
- **Microsoft.Playwright 1.52.0→1.65.1:** Betrifft primär die OsInterface-Lane (Playwright-Browsertests), die lokal nicht läuft und in der CI best-effort ist — ein Bruch würde die reguläre Lane nicht blockieren, ist aber trotzdem zu vermeiden: Recherche-Schritt prüft API-Brüche gegen `Tests/Playwright/Infrastructure/`; Restrisiko wird über die CI-Lane beobachtet.
- **bunit 2.7.2→2.15.2:** Neue bunit-Version kann andere transitive AngleSharp-/AngleSharp.Css- und WebAssembly-Anforderungen ziehen; der AngleSharp-Override-Kommentar muss nach dem Update gegen die tatsächliche Auflösung geprüft werden.
- **ShadcnBlazor 1.0.14→1.0.22:** UI-Komponenten-Patch — theoretisch sichtbare Rendering-Änderungen; abgedeckt durch `LayoutSmokeTests` (BoundingBox) in der regulären Lane.
- **Coverage-Gate:** `coverlet.collector`-Update berührt das 70-%-Line-Coverage-Gate — verifiziert durch den CI-Lauf; lokal nur Testgrün nachgewiesen.
- **Neue transitive Vulnerabilities:** Updates können neue transitive Befunde ziehen → Security-Scan-Gate würde rot. Lokaler Scan nach jedem Batch; Befund → Update zurückstufen oder Direkt-Pin erwägen.

## Umsetzungsreihenfolge

1. **Recherche + finale Risikoeinstufung**
   - Voraussetzungen: `inventory/outdated.md`, `inventory/code-usage.md` (vorhanden).
   - Beschreibung: Release Notes / NuGet-Metadaten aller 11 Kandidaten + Zusatzfakten (OData-Preview-Status, Edm-8.x-Patch, xunit.runner.visualstudio-4.x-Kompatibilität). Ergebnis: finale Liste „umsetzen" vs. „dokumentieren"; bei Breaking-Change-Befund Umstufung auf hoch.

2. **Batch A: Patch-Updates umsetzen + verifizieren**
   - Voraussetzungen: Schritt 1 bestätigt alle drei Patches als niedrig.
   - Beschreibung: `ShadcnBlazor`→1.0.22, `Http.Abstractions`→2.3.14, `SignalR.Core`→1.2.14; restore → build → `dotnet test --filter "Category!=OsInterface"` → vulnerable-Scan.

3. **Batch B: Minor-Updates umsetzen + verifizieren**
   - Voraussetzungen: Schritt 1 bestätigt Minoren als mittel; Batch A grün.
   - Beschreibung: `Markdig`→1.6.0, `Jint`→4.18.1 (Infrastructure + Tests), `bunit`→2.15.2, `AngleSharp`→1.10.5 inkl. Kommentar-Update, `coverlet.collector`→10.1.1, `Microsoft.NET.Test.Sdk`→18.12.0, `Microsoft.Playwright`→1.65.1, `Moq`→4.21.0. Anschließend transitive AngleSharp-Auflösung prüfen, volle Verifikation; Testbruch → Bisektion und Umstufung des Verursachers.

4. **Empfehlungstabelle + Report-Artefakt erstellen**
   - Voraussetzungen: finale Kandidatenliste aus Schritt 1–3.
   - Beschreibung: `recommendation-report.md` im Feature-Verzeichnis mit vollständiger Tabelle (umgesetzt + dokumentiert + keine-Aktion-Gruppen); Inhalt für PR-Body aufbereiten.

5. **Folge-Issue für hoch/aufgeschobene Empfehlungen anlegen**
   - Voraussetzungen: Schritt 4.
   - Beschreibung: Ein gebündeltes GitHub-Issue (ohne `automated-nuget-review`-Label) mit den hoch-Risiko-/Folgeempfehlungen und Begründungen.

6. **PR gegen `staging` öffnen und Checks fail-fast beobachten**
   - Voraussetzungen: Schritte 2–5, lokale Verifikation grün.
   - Beschreibung: PR-Body = Empfehlungstabelle + Folge-Issue-Link; Checks per `--watch --fail-fast`/Polling mit Fail-Prüfung überwachen; bei Fail sofort in Diagnose (`gh run view --job <id> --log-failed`).

## Tests

### Neue Tests

Keine — reine Paket-Updates ohne Codeänderung; Regressionstests = bestehende Suite (Festlegung aus `requirement.md`).

### Betroffene bestehende Tests

Keine Anpassung geplant. Erwartung: alle 641 Tests der regulären Lane bleiben grün. Besonders paketberührte Suiten, deren Verhalten ein Update-Bruch zuerst zeigen würde:

| Suite | Deckt Update ab |
|-------|-----------------|
| `Services/EndpointScriptRunnerTests` (+ direkte `Jint`-Referenz im Testprojekt) | `Jint` 4.9.2→4.18.1 |
| `Components/*Tests` (11 bunit-Klassen) | `bunit`→2.15.2, `AngleSharp`→1.10.5 |
| `LayoutSmokeTests` (BoundingBox) | `ShadcnBlazor`→1.0.22 |
| `Integration/*`, alle `dotnet test`-Ausführung | `Microsoft.NET.Test.Sdk`→18.12.0, `Moq`→4.21.0, `coverlet.collector`→10.1.1 |
| `Integration/ODataNotificationIntegrationTests`, `ImportApplyNotificationTests` | `SignalR.Core`→1.2.14 |
| `Services/WindowsCurrentUserService`-Pfade, `ApplicationApiClientTests` (`Mock<IHttpContextAccessor>`) | `Http.Abstractions`→2.3.14 |

### E2E-Tests (primärer Funktionsnachweis)

Keine neuen E2E-Tests erforderlich: Die Anforderung berührt keinen Benutzerfluss — es werden ausschließlich Paketversionen geändert, keine UI-Interaktion kommt hinzu oder ändert sich strukturell. Der einzige UI-nah berührte Punkt (`ShadcnBlazor`-Patch) ist durch die bestehenden `LayoutSmokeTests`-Geometrie-Assertions in der regulären Lane abgesichert; die Playwright/OsInterface-Lane läuft in der CI best-effort und beobachtet Restrisiken des `Microsoft.Playwright`-Updates. Lokale Verifikation beschränkt sich bewusst auf `Category!=OsInterface` (Baseline: 641/641, `inventory/test-results/dotnet-test-baseline.log`).

## Akzeptanzszenarien

| # | Szenario | Auslöser | Ausgangszustand | Schritte | Sichtbare Wirkung | Nachweis |
|---|----------|----------|------------------|----------|-------------------|----------|
| 1 | Happy Path: niedrig/mittel-Updates umgesetzt | Issue #107-Paketprüfung | Repo auf Task-Branch, Baseline 641/641 grün, 0 Vulnerabilities | Recherche → Batch A + Batch B in `.csproj` → restore/build/test/scan → Tabelle + Folge-Issue → PR gegen `staging` | PR enthält die Versionsänderungen; PR-Body enthält risikobewertete Empfehlungstabelle; Folge-Issue referenziert | `dotnet test --filter "Category!=OsInterface"` 641/641 grün; `dotnet list package --vulnerable --include-transitive` 0 Befunde; CI-Gates grün |
| 2 | Ein Minor-Update bricht Build oder Tests | Batch-B-Verifikation | Update angewendet, `dotnet test` zeigt Fehlschlag | Bisektion → Verursacher-Versionsangabe zurücksetzen → Paket als „hoch (empirischer Bruch)" in Tabelle/Folge-Issue dokumentieren → Suite erneut grün | Update ist nicht im PR, aber begründet in der Tabelle | Testsuite wieder 641/641; Tabellenzeile mit Bruch-Begründung |
| 3 | Update zieht neue transitive Vulnerability | Post-Update-Vulnerability-Scan | `dotnet list package --vulnerable` meldet Befund | Verursachendes Update zurücksetzen oder begründeten Direkt-Pin setzen (Muster AngleSharp/Pkcs-Overrides) | Security-Scan-Gate bleibt bestehbar | Scan zeigt wieder 0 Befunde; Entscheidung im PR-Body dokumentiert |
| 4 | Recherche zeigt Breaking Changes bei „mittel"-Kandidat | Schritt-1-Recherche | Release Notes mit Breaking-Change-Eintrag | Kandidat vor Umsetzung auf „hoch" umstufen; nur dokumentieren | Weniger Updates im PR als initial angenommen; Begründung in Tabelle | Tabellenzeile „hoch — aufgeschoben (Folge-Issue)" |

## Offene Punkte

| # | Offener Punkt | Empfohlener Vorschlag |
|---|---------------|------------------------|
| 1 | Sind bei `Jint` 4.18.1, `Microsoft.Playwright` 1.65.1, `Microsoft.NET.Test.Sdk` 18.12.0 und `bunit` 2.15.2 Breaking Changes enthalten, die unsere Nutzung treffen? | Wird in Umsetzungsschritt 1 per Release-Notes-Recherche geklärt; bei Befund Umstufung auf hoch (kein Blocker für den Plan). |
| 2 | Existiert für `Microsoft.AspNetCore.OData` ein stabiler 10.x-Release oder ein neueres Preview — und für `Microsoft.OData.Edm` noch ein 8.x-Patch > 8.4.3? | Wird in Schritt 1 per NuGet-Metadaten geklärt; Ergebnis fließt nur in die Dokumentation (OData bleibt per Schema hoch/unverändert). |
| 3 | Ein gebündeltes Folge-Issue vs. mehrere thematische Folge-Issues für die hoch-Empfehlungen? | Ein gebündeltes Folge-Issue mit thematischen Abschnitten — geringerer Verwaltungsaufwand, bleibt außerhalb der `automated-nuget-review`-Duplikatsprüfung nachverfolgbar. |
