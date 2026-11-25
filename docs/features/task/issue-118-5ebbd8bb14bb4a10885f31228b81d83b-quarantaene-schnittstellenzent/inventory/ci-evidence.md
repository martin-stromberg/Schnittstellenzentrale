# CI-Befund – Fehlgeschlagene Läufe und Trace-Auswertung

Stand der Recherche: 2026-11-25. Quelle: `gh run list` / `gh run download` gegen
`martin-stromberg/Schnittstellenzentrale` (Token-Scopes: `repo`, `workflow`; Artefakt-Download
funktioniert). Heruntergeladene Artefakte liegen unter `inventory/trx-history/`.

## 1. Run-Inventar (os-interface-tests-Job, abgeschlossene Läufe)

Der `os-interface-tests`-Job ist `continue-on-error` — der Workflow-Run selbst meldet
`success`, selbst wenn der Job `failure` ist. Job-Ergebnis via
`gh run view <id> --json jobs` ermittelt.

### pr-staging-ci.yml (Job `os-interface tests`, Artefakt `test-results-os-interface-pr`)

| Run-ID | Job-ID | Job-Ergebnis | TRX-Start (UTC) | Branch | Outcome `OdataApi_Registrieren…` | Outcome `OdataApi_GetEndpoints…` |
|---|---|---|---|---|---|---|
| 45076613896 | 137375286545 | failure | 2026-11-22 22:17 | task/issue-100-… (OsInterface-Härtung) | **Passed** (start 22:17:42) | **Passed** (start 22:17:59) — Failed war `TreeCollapseTests.EndpointGroupInitiallyCollapsed` (Issue #112) |
| 45118489116 | 137506682207 | success | 2026-11-23 05:35 | task/issue-100-… | Passed (05:35:59) | Passed (05:36:11) |
| 45271966258 | 138033893919 | success | 2026-11-23 21:41 | task/issue-107-… | Passed (21:42:08) | Passed (21:42:18) |
| 45273020079 | 138037559847 | success | 2026-11-23 21:48 | task/issue-107-… | Passed (21:48:17) | Passed (21:48:27) |
| 45336591664 | 138250201424 | success | 2026-11-24 05:57 | task/issue-108-… | Passed (05:57:26) | Passed (05:57:17) — **GetEndpoints lief zuerst** |
| **45434716126** | 138589400722 | **failure** | 2026-11-24 16:17 | task/issue-107-… | **FAILED (16:17:26)** | Passed (16:17:16) — **GetEndpoints lief zuerst** |
| **45543708029** | 138968184988 | **failure** | 2026-11-25 04:01 | task/issue-112-… (PR #117) | **FAILED (04:02:14)** | Passed (04:02:03) — **GetEndpoints lief zuerst** |

Weitere abgeschlossene pr-staging-ci-Läufe im Zeitraum: 45335797907/45334831871
(cancelled/failure vor os-interface-Job bzw. ohne OS-TRX), 45472190879
(os-interface-Job `skipped` — Backmerge-PR, `is_backmerge=true`).

### staging-ci.yml (Job `os-interface tests`, Artefakt `test-results-os-interface-staging`)

| Run-ID | Job-ID | Job-Ergebnis | TRX-Start (UTC) | Outcome `OdataApi_Registrieren…` | Outcome `OdataApi_GetEndpoints…` |
|---|---|---|---|---|---|
| 45129787523 | 137542452724 | success | 2026-11-23 07:17 | Passed (07:18:00) | Passed (07:18:12) |
| 45338564605 | 138256447182 | success | 2026-11-24 06:13 | Passed (06:13:54) | Passed (06:13:32) — GetEndpoints lief zuerst |
| 45436125872 | 138594421034 | success | 2026-11-24 16:23 | Passed (16:23:50) | Passed (16:23:41) — GetEndpoints lief zuerst |
| 45472941743 | 138725572205 | skipped | – | – | – |
| **45550953861** | 138991769842 | success | 2026-11-25 05:05 | Passed (05:06:10) | Passed (05:06:01) — GetEndpoints lief zuerst |

## 2. Zuordnung zum Analyzer-Befund

- Der Quarantäne-Issue (#118) wurde am **2026-11-25T04:05:36Z** angelegt — zeitlich passend
  zum `Create quarantine issues`-Schritt des os-interface-Jobs von Run **45543708029**
  (Job-Start 03:59:50Z, TRX-Start 04:01:53Z). Issue-Autor: CI (`gh issue create` via
  `New-FlakyQuarantineIssue.ps1`).
- `Download-OsInterfaceTrxHistory.ps1` holt nur Artefakte **desselben Workflows** — die
  Kennzahlen (9 Ausführungen, 2 Fehlschläge, 22,2 %) stammen daher aus dem
  **pr-staging-ci-Fenster** (`test-results-os-interface-pr`), nicht aus staging-ci.
- Im PR-Workflow-Fenster gab es zwischen Run 45434716126 und 45543708029 **keinen grünen
  Primär-Lauf** (45472190879 hatte den Job `skipped`) → die zwei Fehlschläge sind
  aufeinanderfolgend → 0 Flake-Ereignisse konsistent mit dem Analyzer-Report.
- In **staging-ci** lief der Test in allen beobachteten Läufen grün (zuletzt
  Run 45550953861, ~1 h nach dem Issue).

## 3. Fehlermeldung (identisch in beiden Fehlschlägen)

Aus `run-45434716126/test-results-os-interface.trx` und
`run-45543708029/test-results-os-interface.trx`, `UnitTestResult` für
`…ODataEndpointExecutionTests.OdataApi_Registrieren_Importieren_UndAlleEndpunkteErfolgreichAusfuehren`:

```
Microsoft.Playwright.PlaywrightAssertionException : Locator expected to be visible
Error: strict mode violation: Locator(".sz-tree-item-btn").Filter(new() { HasText = "SZ-OData-E2E" }) resolved to 2 elements:
    1) <button title="SZ-OData-E2E" class="sz-tree-item-btn">SZ-OData-E2E</button> aka GetByRole(AriaRole.Button, new() { Name = "SZ-OData-E2E" }).First
    2) <button title="SZ-OData-E2E" class="sz-tree-item-btn">SZ-OData-E2E</button> aka GetByRole(AriaRole.Button, new() { Name = "SZ-OData-E2E" }).Nth(1)

Call log:
  - Expect "ToBeVisibleAsync" Locator(".sz-tree-item-btn").Filter(new() { HasText = "SZ-OData-E2E" }) with timeout 5000ms
  - waiting for Locator(".sz-tree-item-btn").Filter(new() { HasText = "SZ-OData-E2E" })
```

StackTrace (beide Runs identisch):

```
at Microsoft.Playwright.Core.AssertionsBase.ExpectImplAsync(...) in /_/src/Playwright/Core/AssertionsBase.cs:line 95
at Microsoft.Playwright.Core.AssertionsBase.ExpectImplAsync(...) in /_/src/Playwright/Core/AssertionsBase.cs:line 69
at Schnittstellenzentrale.Tests.Playwright.ODataEndpointExecutionTests.OdataApi_Registrieren_Importieren_UndAlleEndpunkteErfolgreichAusfuehren()
    in ...\src\Schnittstellenzentrale.Tests\Playwright\ODataEndpointExecutionTests.cs:line 47
```

- **Fehlschlagender Schritt:** Zeile 47 — die allererste Baum-Assertion nach dem
  „Speichern"-Klick der Anwendungsanlage. Der Test kam nie bis zum OData-Import,
  Chevron-Expand oder einer Endpunkt-Ausführung.
- Der Strict-Mode-Verstoß schlägt **sofort** fehl (kein 5-s-Timeout): Testlaufzeit
  ~2 s (TRX-`startTime` 04:02:14, Trace-Stop 04:02:16; die letzte Log-Zeile im
  Trace steht bei ~1,73 s nach Seitenstart).

## 4. Trace-Auswertung (`playwright-traces-pr`, Run 45543708029)

Artefakt `playwright-traces-pr` heruntergeladen; die zwei ODataEndpointExecutionTests-ZIPs:

| Trace-ZIP | Test | Ende der Aktionen |
|---|---|---|
| `ODataEndpointExecutionTests-040213316.zip` | `OdataApi_GetEndpoints_MitContainsFilter…` | läuft bis `Expect ToContainTextAsync .sz-response-body-pre` (bestanden) |
| `ODataEndpointExecutionTests-040216019.zip` | `OdataApi_Registrieren_Importieren_…` | bricht beim `Expect ToBeVisibleAsync .sz-tree-item-btn HasText "SZ-OData-E2E"` ab |

Extrahiert nach `inventory/trx-history/trace-inspect-45543708029/`. DOM-Belege:

### 4a. Früher Snapshot (Basis-HTML `resources/14301b1acfbce6de348afe26d412fe9cfba51c01.html`)

Baum „Ohne Sammlung" (aufgeklappt, `sz-icon-chevron-down`) enthält zu diesem
Aufnahmezeitpunkt **zwei** ungruppierte Anwendungszeilen:

```html
<div class="sz-tree-children">
  <div><div class="sz-tree-row" draggable="true" …>
    <button class="sz-tree-chevron-btn"><i class="sz-icon sz-icon-chevron-right"></i></button>
    <i class="sz-icon sz-icon-window sz-tree-icon"></i>
    <button class="sz-tree-item-btn" title="SZ-OData-E2E">SZ-OData-E2E</button> …
  </div></div>
  <div><div class="sz-tree-row" draggable="true" …>
    <button class="sz-tree-chevron-btn">…</button>
    <i class="sz-icon sz-icon-window sz-tree-icon"></i>
    <button class="sz-tree-item-btn" title="Filter-Test-App">Filter-Test-App</button> …
  </div></div>
</div>
```

**`Filter-Test-App` wird ausschließlich vom Geschwistertest
`OdataApi_GetEndpoints_MitContainsFilter_…` angelegt**
(`ODataEndpointExecutionTests.cs:144`). Ihr Vorhandensein im DOM des
`Registrieren`-Tests belegt: **die In-Memory-Datenbank war beim Teststart nicht
zurückgesetzt** — `TestDatabaseSeeder.ResetAsync()` (`EnsureDeletedAsync` +
`EnsureCreatedAsync`, `TestDatabaseSeeder.cs:34-35`) hatte die Daten des vorherigen
Facts nicht entfernt.

### 4b. Letzter Frame-Snapshot (`frame-snapshot`, callId `bnxq@27`, Phase `after`, Trace-Timestamp 1793.7)

Der Inline-DOM des letzten Snapshots zeigt im Abschnitt „Ohne Sammlung"
**zwei** `sz-tree-row`-Zeilen, jede mit einem eigenen
`BUTTON { class: "sz-tree-item-btn", title: "SZ-OData-E2E" }` — die Alt-Zeile aus
dem Geschwistertest plus die neu angelegte. Exakt die beiden Elemente, die der
Strict-Mode-Verstoß meldet.

### 4c. Zweiter Fehlschlag (Run 45434716126)

`trace-inspect-45434716126/` (`ODataEndpointExecutionTests-161728381.zip`):
dasselbe Bild — Basis-HTML enthält `SZ-OData-E2E`=2 + `Filter-Test-App`=2
(je 1 Button, also Datenstand mit Alt-App); letzter `after`-Snapshot
(timestamp 1740.8) enthält 2 `sz-tree-item-btn`-Elemente.

## 5. Ausführungsreihenfolge der Facts (TRX-`startTime`)

| Run | Reihenfolge | `Registrieren`-Ergebnis |
|---|---|---|
| 45076613896, 45118489116, 45129787523, 45271966258, 45273020079 | Registrieren → GetEndpoints | Passed |
| 45336591664, 45338564605, 45436125872, 45550953861 | GetEndpoints → Registrieren | **Passed** |
| 45434716126, 45543708029 | GetEndpoints → Registrieren | **FAILED** |

Beobachtung: Die Fact-Reihenfolge variiert zwischen Läufen (kein Test-Orderer im
Projekt konfiguriert, `grep Orderer` → keine Treffer). In 4 Läufen lief
`Registrieren` ebenfalls als Zweites (nach `GetEndpoints`) und bestand — **nicht**
weil der Reset wirkt: `EnsureDeletedAsync` ist auf der Shared-Cache-In-Memory-DB
ein deterministischer No-op (`DataSource` leer bei `Mode=Memory`; Verbindungen
non-pooled), Alt-Daten sind in **allen** Läufen nach dem Erstfact vorhanden (auch
im DOM grüner Läufe belegt). Die grünen Läufe erklären sich durch eine
DOM-/Render-Race: `Expect` an Zeile 47 resolved zu 1 Element, bevor das Re-Render
die Duplikat-Zeile anhängt; erst bei verspätetem Re-Render (CI-Last) entsteht der
Strict-Mode-Verstoß. (Korrektur per plan-check.1.md — zuvor stand hier fälschlich
„Reset meist wirksam".)

## 6. Was der Befund ausschließt

- **Kein Chevron-/Expand-Timeout** (der #112-Mechanismus): Der Test scheiterte bei
  Zeile 47, bevor irgendein Chevron-Klick (erste Verwendung Zeile 59) erreicht wurde.
- **Kein `WaitForServerReady`-/Navigations-Timeout**: Page-Load + Formular-Befüllung
  liefen durch (Trace-Logs zeigen erfolgreiche Navigation, Fills und Speichern-Klick).
- **Kein Produktfehler in der Endpunkt-Ausführung**: Authenticate/GET-Requests wurden
  nie erreicht.
- **Keine klassische Retry-Flake**: beide Fehlschläge identische Signatur,
  aufeinanderfolgend im PR-Workflow-Fenster.

## 7. Weitere Artefakte

- `inventory/trx-history/run-<id>/test-results-os-interface.trx` — die 11 Primär-TRX
  des Auswertungsfensters (beide Workflows). Kein `*-flaky-retry.trx` vorhanden —
  der Test ist nicht `[Flaky]`-markiert, Retry-Step blieb daher No-op.
- `inventory/trx-history/trace-inspect-*/` — entpackte Playwright-Traces der beiden
  fehlgeschlagenen Facts (trace.trace, trace.network, frame-snapshots, Ressourcen).
- `inventory/trx-history/{inspect_trace,dump_html,count_bytes}.py` — kleine
  Inspektionsskripte (Windows-Langpfad via `\\?\`-Präfix).
