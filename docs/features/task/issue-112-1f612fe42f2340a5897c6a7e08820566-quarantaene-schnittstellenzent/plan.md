# Umsetzungsplan: Quarantäne `TreeCollapseTests.EndpointGroupInitiallyCollapsed`

## Übersicht

Bearbeitung des Quarantäne-Issues #112 für den Playwright-Test `EndpointGroupInitiallyCollapsed`: Das konkrete Fehlerbild ist aus den CI-Artefakten bereits ermittelt (siehe Befund unten) — ein verlorener Gruppen-Chevron-Klick nach `Page.ReloadAsync()` auf dem Code-Stand vor der `ExpandSystemGroupAsync`-Härtung. Umgesetzt werden die verbliebenen gleichartigen Lücken (Blindklick auf die Anwendungszeile, fehlende Speicher-Abschluss-Warte vor dem Reload), die `[Flaky]`-Markierung sowie Issue-Dokumentation und CI-Beobachtung. Betroffen ist ausschließlich `src/Schnittstellenzentrale.Tests/Playwright/TreeCollapseTests.cs` — kein Produktcode, keine Infrastruktur.

### Ermitteltes Fehlerbild (Befund aus CI-Artefakt-Analyse)

Die TRX-Artefakte (`test-results-os-interface-pr`) der jüngsten CI-Läufe wurden via `gh run download` ausgewertet. Der dokumentierte Fehlschlag stammt aus **Run 45076613896** (`pr-staging-ci.yml`, 2026-11-22T22:18 UTC, Branch `task/issue-100-…-flaky-test-haertung-umgebungss`, Code-Stand Commit `0095a17`):

- **Fehler:** `System.TimeoutException : Timeout 30000ms exceeded` an `TreeCollapseTests.cs:206` — dort stand `await appBtn.ClickAsync()`.
- **Wartendes Element:** `Locator(".sz-tree-row").Filter(Has: .sz-tree-item-btn mit HasText "Schnittstellenzentrale").Locator(".sz-tree-item-btn")` — die `SystemAppRow` wurde **nie sichtbar**.
- **Ursache:** Die damalige `ExpandSystemGroupAsync` bestand aus einem einzelnen Blindklick `groupChevron.ClickAsync()` ohne Ergebnisverifikation. Der Klick ging während des SignalR-Reconnects nach `Page.ReloadAsync()` verloren → die Systemgruppe blieb zugeklappt → die Anwendungszeile wurde nie gerendert → 30-s-Timeout. Damit ist **Hypothese 1 (verlorener UI-Event-Klick) in der CI bewiesen** — allerdings auf Gruppen- statt Anwendungsebene.
- **Korrekturstatus:** Genau diese Lücke wurde durch Commit `b706a89` (2026-11-23, „`ExpandSystemGroupAsync` verifiziert Expand-Ergebnis statt Blindklick") geschlossen — **nach** dem Fehlschlag und **vor** dem Analyzer-Befund. Alle seither ausgewerteten Läufe (45273020079, 45334831871, 45336591664, 45338564605, 45436125872 sowie die lokale Baseline 94/94) zeigen den Test grün; die 20-%-Fehlrate ist ein Artefakt des Auswertefensters mit dem Pre-Fix-Fehlschlag.
- **Verbleibende gleichartige Lücke:** Der Blindklick `appBtn.ClickAsync()` (Zeile 226) expandiert die Anwendungszeile ohne Verifikation — derselbe Verlustmechanismus eine Ebene tiefer. Geht dieser Klick verloren, feuern `SelectAndExpandApplication`/`ToggleApplicationExpanded`/`ReloadApplicationDataAsync` nie, die Ordnerzeile wird nie gerendert und die Assertion auf `sz-icon-chevron-right` läuft in den (5-s-)Assertion-Timeout.
- Trace-ZIP des Fehlschlags liegt als `TreeCollapseTests-221833010.zip` im CI-Artefakt `playwright-traces-pr` von Run 45076613896 vor (Test endete 22:18:33 UTC) und kann bei Bedarf mit `npx playwright show-trace` verifiziert werden.

## Scope-Klärung

| Feld | Festlegung |
|------|------------|
| Verarbeitungseinheit | Ein Quarantäne-Vorgang für genau einen OS-Schnittstellen-Test: `Schnittstellenzentrale.Tests.Playwright.TreeCollapseTests.EndpointGroupInitiallyCollapsed` — Fehlerbild-Analyse (abgeschlossen, Befund oben), Schließen der Restlücken, `Flaky`-Markierung, Wiederbeobachtung. |
| Auslösende Ereignisse | Automatischer Analyzer-Befund (1 Fehlschlag in 5 Ausführungen = 20 %, exakt am Schwellwert) mit dedupliziertem Issue #112; fachlicher Auslöser im Test: `Page.ReloadAsync()` mit anschließender Expand-Sequenz während des SignalR-Reconnects. |
| Erwartete Wirkung | Der Test schlägt nicht mehr wegen verlorener UI-Events oder verfrühter Reloads fehl; ein etwaiger Restfehlschlag wird durch `[Flaky]` genau einmal automatisch wiederholt (`Retry-FlakyTests.ps1`) und bleibt als `*-flaky-retry.trx` sichtbar statt blockierend. Das Fehlerbild ist im Issue dokumentiert. |
| Umgang mit fehlenden Objekten | CI-Artefakte: bereits beschafft und ausgewertet (Run 45076613896, TRX + Trace-ZIP) — wiederverwenden, nicht erfinden; bei späterer Expiry entfällt nur die optionale Trace-Nachsicht. `FlakyAttribute`: vorhanden (`Infrastructure/Testing/FlakyAttribute.cs`), wiederverwenden. Trace-ZIPs: werden pro Lauf automatisch geschrieben — wiederverwenden. `ExpandSystemGroupAsync`-Muster: wiederverwenden als Vorlage für den neuen Anwendungs-Expand-Helper. |

### Scope-Ausschlüsse

| # | Einschränkung / Ausschluss | Begründung | Status |
|---|----------------------------|------------|--------|
| 1 | Keine Änderung am Produktverhalten (Baum-Expand, SignalR-Hub, API) | Expand-Zustände sind komponenteninterner Speicher ohne Persistenz; Chevron wird auch für leere Ordner gerendert; der CI-Befund zeigt einen Testfehler (verlorener Klick), keinen fachlichen Defekt. | bestätigt (2026-11-24, Anwender: „weiter so" via Issue #112) |
| 2 | Keine Umbaumaßnahmen an Quarantäne-/Retry-Infrastruktur (`Analyze-FlakyTestResults.ps1`, `Retry-FlakyTests.ps1`, `New-FlakyQuarantineIssue.ps1`, Workflows) | Infrastruktur funktioniert wie vorgesehen — hat Issue und Einmal-Retry bereits korrekt erzeugt. | bestätigt (2026-11-24, Anwender: „weiter so" via Issue #112) |
| 3 | Keine `[Flaky]`-Markierung der Geschwistertests `ClickEndpointGroupName_TogglesCollapse` und `ClickEndpointGroupChevron_TogglesCollapse` | Vergaberegel: nur mit dokumentiertem Befund; beide liefen in allen ausgewerteten Läufen grün. Die identische Klick-Lücke wird ohne Markierung mitgehärtet. | bestätigt (2026-11-24, Anwender: „weiter so" via Issue #112) |
| 4 | Blindklicks in `ClickApplicationName_*`-Tests (Zeilen 64, 84, 106, 109) bleiben unverändert | Gleicher Mechanismus, aber kein `ReloadAsync`-Kontext (Circuit bereits etabliert), kein dokumentierter Fehlschlag; Anforderung nennt explizit nur die beiden `ClickEndpointGroup*`-Geschwister. | bestätigt (2026-11-24, Anwender: „weiter so" via Issue #112) |
| 5 | `SiteScreenshotTests` schreiben `site/screenshots/*.png` bei jedem OS-Lauf neu | Nebenbefund aus dem Inventory-Lauf; unabhängiges Problem (Test-Seiteneffekt ins Repo-Arbeitsverzeichnis), nicht Teil dieses Quarantäne-Vorgangs — ggf. separates Issue. | bestätigt (2026-11-24, Anwender: „weiter so" via Issue #112) |
| 6 | Eintrag unter „Bekannte Fehlerbilder" in `docs/help/stabilitaet/os-interface-tests.md` nur bei erneutem Fehlschlag | Anforderung legt die Doku-Pflicht auf den Issue-Kommentar (jetzt) und den Doku-Abschnitt (bei Wiederholung). | bestätigt (2026-11-24, Anwender: „weiter so" via Issue #112) |

## Designentscheidungen

| Komponente / Bereich | Gewählter Ansatz | Begründung |
|----------------------|-----------------|------------|
| Expand-Verifikation Anwendungszeile | Neuer klasseninterner Helper `ExpandSystemApplicationAsync` (parameterlos, auf `SystemAppRow` operierend) — exaktes Spiegelbild von `ExpandSystemGroupAsync` | Konsistenz mit dem bewährten Geschwister-Muster; alle drei Aufrufstellen expandieren dieselbe Zeile, ein `ILocator`-Parameter wäre ungenutzte Generalität. |
| Warte-Ziel des Helpers | `sz-icon-chevron-down` innerhalb `SystemAppRow` statt der Ordnerzeile | Der Chevron ist das direkte Render-Ergebnis von `_expandedApplicationIds` — verifiziert den Klick-Effekt selbst, nicht die REST-Nachladelatenz (die deckt ein erhöhtes Assertion-Timeout auf der Ordnerzeile ab; die Geschwistertests warten ohnehin per `ClickAsync`-Default (30 s) auf ihre Zielelemente). |
| `[Flaky]`-Vergabe | `[Flaky]` auf Methodenebene über `[Fact]` — jetzt, trotz bereits erfolgter Teil-Korrektur | Beide Vergabekriterien liegen vor (Analyzer-Befund + dokumentierter CI-Fehlschlag); die Befundlage von 1/5 ist dünn und der Restmechanismus wird präventiv geschlossen — der Einmal-Retry schützt während des Beobachtungsfensters. Entfernung erfolgt erst beim Issue-Schluss nach stabilem Fenster (Vergaberegel „entfernen, sobald die Ursache behoben ist"). |
| Geschwistertests | Mithärten (Blindklick → `ExpandSystemApplicationAsync`), ohne `[Flaky]` | Identischer Fehlermechanismus, ein Helper-Aufruf Differenz, von der Anforderung explizit erlaubt; verhindert ein künftiges zweites Quarantäne-Issue mit identischem Befund. Risiko ohne Reload ist geringer, aber nicht null (Klick direkt nach Ordner-Anlage-Dialog kann in Rest-Reconnect-Phasen fallen). |
| Speicher-Abschluss vor Reload | Zustandsbasierte Warte (Dirty-Badge sichtbar → Speichern-Klick → Badge verschwunden + kein `.sz-alert-danger`/`.sz-error`) statt `Task.Delay` | Das Verschwinden von `.sz-endpoint-dirty-badge` ist das sichtbare Ende von `OnSaveSuccessAsync` — deckt REST-Write **und** `DisableBeforeUnloadGuardAsync` ab und schließt damit auch den offenen `beforeunload`-Blockadepfad ohne dessen Browser-Semantik klären zu müssen. |

## Programmabläufe

### Gehärteter Testablauf `EndpointGroupInitiallyCollapsed`

1. `Page.GotoAsync(BaseUrl)` + `WaitForLoadStateAsync(LoadState.NetworkIdle)` — unverändert.
2. `ExpandSystemGroupAsync()` — unverändert (bereits gehärtet).
3. Ordner „Initial-Collapsed-Ordner" über das Kontextmenü der Systemanwendung anlegen — unverändert.
4. „Endpunkt anlegen" über dasselbe Kontextmenü; `GetByPlaceholder("Relativer Pfad").FillAsync("/api/test")` — unverändert.
5. **Neu:** Sichtbarkeit von `.sz-endpoint-dirty-badge` abwarten (verifiziert, dass `MarkDirty`/`_isDirty` registriert wurde).
6. „Speichern"-Klick — unverändert.
7. **Neu:** Verschwinden von `.sz-endpoint-dirty-badge` abwarten (spezifischer Timeout > Assertion-Default, z. B. 15 s) und Abwesenheit von `.sz-alert-danger` sowie `.sz-error` prüfen — garantiert: `UpdateEndpointAsync`-Write abgeschlossen, `beforeunload`-Guard aufgehoben.
8. `Page.ReloadAsync()` — kann jetzt nicht mehr durch den Guard blockiert/verzögert werden.
9. `ExpandSystemGroupAsync()` — unverändert.
10. **Geändert:** `ExpandSystemApplicationAsync()` statt `appBtn.ClickAsync()` — verifizierter Expand mit Retry.
11. Assertion auf `groupRow`-`sz-icon-chevron-right` — unverändertes Ziel, **aber mit erhöhtem Assertion-Timeout** (z. B. 15 s), damit die REST-Nachladelatenz der Ordnerdaten (`ReloadApplicationDataAsync`) unter CI-Last abgedeckt ist.

Beteiligte Klassen/Komponenten: `TreeCollapseTests`, `ApplicationGroupTree`, `CollapsibleSection`, `EndpointPage`, `WorkspacesLayout`, `PlaywrightTestBase`.

### Helper-Ablauf `ExpandSystemApplicationAsync`

1. Locators: `appRow = SystemAppRow`, `appBtn = appRow.Locator(".sz-tree-item-btn")`, Indikator `expandedChevron = appRow.Locator(".sz-tree-chevron-btn .sz-icon-chevron-down")`.
2. Bis zu 3 Versuche: `expandedChevron.WaitForAsync(Timeout = 5_000)` → bei Erfolg Rückkehr; bei `TimeoutException` → `appBtn.ClickAsync()` und nächster Versuch.
3. Nach 3 Versuchen finaler `expandedChevron.WaitForAsync()` mit Default-Timeout (30 s) — schlägt dann sauber mit dem echten Restproblem fehl.
4. Wichtig: Die Prüfung steht **vor** jedem Klick — ein bereits expandierter Zustand wird nicht durch einen überflüssigen Klick wieder zugeklappt (`ToggleApplicationExpanded` toggelt).

Beteiligte Klassen/Komponenten: `TreeCollapseTests`, `ApplicationGroupTree` (`ToggleApplicationExpanded`, `ReloadApplicationDataAsync`).

### Analyse- und Dokumentationsablauf (bereits weitgehend erfolgt)

1. TRX-Artefakte der jüngsten Läufe via `gh run download` ziehen und den `Failed`-Eintrag lokalisieren — **erledigt** (Run 45076613896).
2. Fehlerbild aus `<ErrorInfo>`/StackTrace bestimmen — **erledigt** (30-s-Timeout auf `SystemAppRow`, verlorener Gruppen-Chevron-Klick).
3. Optional: Trace `TreeCollapseTests-221833010.zip` mit `npx playwright show-trace` sichten (Bestätigung, dass die Systemgruppe nach Reload zugeklappt blieb).
4. Befund als Kommentar in Issue #112 dokumentieren (`gh issue comment`) — in der Umsetzung.

Beteiligte Komponenten: CI-Artefakte `test-results-os-interface-*`, `playwright-traces-*`, GitHub Issue #112.

## Neue Klassen

Keine — es entsteht ausschließlich eine neue private Hilfsmethode in der bestehenden Testklasse.

## Änderungen an bestehenden Klassen

### `TreeCollapseTests` (Testklasse, `src/Schnittstellenzentrale.Tests/Playwright/TreeCollapseTests.cs`)

- **Neue Methoden:**
  - `ExpandSystemApplicationAsync` — verifizierender Expand der Systemanwendungszeile; bis zu 3 Versuche mit 5-s-Sichtbarkeitsprüfung auf `sz-icon-chevron-down`, sonst `appBtn.ClickAsync()`; finaler `WaitForAsync()` mit Default-Timeout. Rückgabewert `Task`, keine Parameter. Muster: `ExpandSystemGroupAsync` (Zeilen 118–138).
- **Geänderte Methoden:**
  - `EndpointGroupInitiallyCollapsed` (Zeilen 203–230): (a) `[Flaky]` über `[Fact]` ergänzen; (b) nach dem Füllen des Pfad-Felds Sichtbarkeit von `.sz-endpoint-dirty-badge` abwarten; (c) nach dem „Speichern"-Klick auf Verschwinden des Badges sowie Abwesenheit von `.sz-alert-danger`/`.sz-error` warten; (d) `appBtn.ClickAsync()` (Zeile 226) durch `ExpandSystemApplicationAsync()` ersetzen; (e) Assertion-Timeout auf die Ordnerzeile erhöhen.
  - `ClickEndpointGroupName_TogglesCollapse`: `appBtn.ClickAsync()` (Zeile 160) durch `ExpandSystemApplicationAsync()` ersetzen.
  - `ClickEndpointGroupChevron_TogglesCollapse`: `appBtn.ClickAsync()` (Zeile 189) durch `ExpandSystemApplicationAsync()` ersetzen.
- **Neue Attribute:** `[Flaky]` auf `EndpointGroupInitiallyCollapsed` (Methodenebene; `OsInterface` kommt von der Klasse — Kombination durch `FlakyConventionTests` abgesichert).

Keine weiteren Klassen werden geändert. `FlakyAttribute`, `FlakyConventionTests`, `PlaywrightTestBase`, alle Produktkomponenten und alle Skripte/Workflows bleiben unverändert.

## Datenbankmigrationen

Keine.

## Validierungsregeln

Keine — es werden keine neuen oder geänderten Eingaben verarbeitet (reine Testhärtung).

## Konfigurationsänderungen

Keine — Analyzer-Schwellwerte und Workflows bleiben unverändert.

## Seiteneffekte und Risiken

- **Retry-Laufzeit:** `[Flaky]` aktiviert den Einmal-Retry im `os-interface-tests`-Job. Bei einem künftigen Fehlschlag verdoppelt sich die Laufzeit dieses Tests (aktuell ~12 s) — gewollt und vom Job-Timeout (45 min) mühelos gedeckt.
- **Filter-Menge `Category=Flaky`:** enthält künftig genau einen Test (bisher null). `Retry-FlakyTests.ps1` listet die Menge via `--list-tests` — korrekte Trait-Auswertung ist durch `ITraitAttribute.GetTraits` und `TestTraitReader` gegeben.
- **Reguläre Lane:** unberührt — `[Flaky]` verändert die Lane-Zuordnung nicht (`Category!=OsInterface` schließt den Test weiter aus, `Category=OsInterface` schließt ihn weiter ein). `FlakyConventionTests` und `TraitParityTests` laufen mit und bleiben grün, da `OsInterface` klassenweit vererbt.
- **Nebenbefund (nicht im Scope):** `SiteScreenshotTests` schreiben `site/screenshots/*.png` bei jedem OS-Lauf neu — lokale Läufe hinterlassen git-Änderungen, die zurückgesetzt werden müssen. Als Scope-Ausschluss dokumentiert; ggf. separates Follow-up-Issue.
- **Restrisiko:** Die Befundlage (1 Fehlschlag in 5 Läufen, exakt am Schwellwert) ist dünn; der Fix schließt den bewiesenen Mechanismus plus die strukturell identische Restlücke, kann aber eine unbekannte Restursache nicht ausschließen — deshalb `[Flaky]` + Beobachtungsfenster, und der `Flaky`-Trait bleibt bis zum Nachweis anhaltender Stabilität erhalten.

## Umsetzungsreihenfolge

1. **Befund im Issue #112 dokumentieren**
   - Voraussetzungen: `gh`-CLI mit Repo-Zugriff (vorhanden); CI-Artefakte bereits lokal ausgewertet.
   - Beschreibung: `gh issue comment 112` mit dem Fehlerbild (Run 45076613896, Timeout auf `SystemAppRow`, verlorener Gruppen-Chevron-Klick auf Code-Stand `0095a17`, Fix-Commit `b706a89`, Restlücke App-Expand). Optional vorab: Trace `TreeCollapseTests-221833010.zip` aus `playwright-traces-pr` des Laufs mit `npx playwright show-trace` sichten und den Befund bestätigen.

2. **Helper `ExpandSystemApplicationAsync` in `TreeCollapseTests` ergänzen**
   - Voraussetzungen: Keine — Muster (`ExpandSystemGroupAsync`) und Locator (`SystemAppRow`) existieren.
   - Beschreibung: Neue private Methode nach dem Helper-Abschnitt (bei Zeile 138) einfügen; Warte-Ziel `SystemAppRow`-`sz-icon-chevron-down`, bis zu 3 Klick-Versuche, finaler `WaitForAsync()`.

3. **`EndpointGroupInitiallyCollapsed` härten und markieren**
   - Voraussetzungen: Schritt 2 (Helper).
   - Beschreibung: `[Flaky]` über `[Fact]`; Dirty-Badge-Sichtbarkeit nach Pfad-Eingabe; Badge-Verschwinden + Fehlerfreiheit (`.sz-alert-danger`, `.sz-error`) nach „Speichern" vor `ReloadAsync`; Blindklick durch `ExpandSystemApplicationAsync` ersetzen; Assertion-Timeout auf die Ordnerzeile erhöhen.

4. **Geschwistertests mithärten**
   - Voraussetzungen: Schritt 2 (Helper).
   - Beschreibung: In `ClickEndpointGroupName_TogglesCollapse` (Zeile 160) und `ClickEndpointGroupChevron_TogglesCollapse` (Zeile 189) den Blindklick `appBtn.ClickAsync()` durch `ExpandSystemApplicationAsync()` ersetzen.

5. **Lokale Verifikation beider Lanes**
   - Voraussetzungen: Schritte 2–4; Chromium-Installation vorhanden (`InstallPlaywright`-Target beim Build); `CI=true` für Headless-Lauf.
   - Beschreibung: `dotnet build`, dann `dotnet test --project src/Schnittstellenzentrale.Tests/… -- --filter "FullyQualifiedName~TreeCollapseTests"`; anschließend `Category=OsInterface` komplett und `Category!=OsInterface` (muss `FlakyConventionTests`/`TraitParityTests` grün halten). `site/screenshots/*.png`-Neuschreibungen nach dem OS-Lauf per `git checkout` zurücksetzen.

6. **Commit/PR und CI-Beobachtung**
   - Voraussetzungen: Schritte 1–5.
   - Beschreibung: Commit mit Closing-Keyword `Closes #112` im Body — im Staging-Workflow schließt das Issue erst bei Promotion nach `main`, was der Liefer-Semantik entspricht. Die Beobachtung (Schritt 7) läuft davon unabhängig: Flakt der Test erneut, legt der Analyzer mangels offenem Issue ein neues dedupliziertes Quarantäne-Issue an. PR-Checks per `gh pr checks <nr> --watch --fail-fast` überwachen.

7. **Beobachtungsfenster und Follow-up**
   - Voraussetzungen: Merge auf `staging` (bzw. später `main`).
   - Beschreibung: Analyzer-Lage über die nächsten CI-Läufe prüfen (TRX-Historie / `Analyze-FlakyTestResults.ps1`-Report im `os-interface-tests`-Job). Bei anhaltender Stabilität (≥ 5 Primär-Läufe ohne Fehlschlag): `[Flaky]` wieder entfernen (Vergaberegel). Bei erneutem Fehlschlag: neues Fehlerbild analysieren und unter „Bekannte Fehlerbilder" in `docs/help/stabilitaet/os-interface-tests.md` ergänzen.

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `ExpandSystemApplicationAsync` (Hilfsmethode) | `TreeCollapseTests` | Stellt den verifizierten Expand der Systemanwendungszeile bereit (Sichtbarkeitsprüfung `sz-icon-chevron-down` vor jedem Klick, bis zu 3 Versuche); federt verlorene UI-Events während SignalR-Reconnects ab. |

Keine neuen Testmethoden — die Anforderung härtet einen bestehenden Test und fügt keine neue Funktionalität hinzu. Die Konformität der `[Flaky]`-Markierung wird durch den bestehenden Meta-Test `FlakyConventionTests` abgesichert.

### Betroffene bestehende Tests

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| `TreeCollapseTests.EndpointGroupInitiallyCollapsed` | Quarantäne-Objekt: `[Flaky]`-Markierung, Speicher-Abschluss-Warte vor `ReloadAsync`, verifizierter App-Expand statt Blindklick, erhöhtes Assertion-Timeout auf der Ordnerzeile. |
| `TreeCollapseTests.ClickEndpointGroupName_TogglesCollapse` | Gleiche Blindklick-Lücke (Zeile 160): `ExpandSystemApplicationAsync` ersetzt `appBtn.ClickAsync()`. |
| `TreeCollapseTests.ClickEndpointGroupChevron_TogglesCollapse` | Gleiche Blindklick-Lücke (Zeile 189): `ExpandSystemApplicationAsync` ersetzt `appBtn.ClickAsync()`. |
| `FlakyConventionTests.FlakyMarkedTests_AlwaysCarryOsInterface` | Läuft mit — verifiziert die neue `Flaky`-+`OsInterface`-Kombination; keine Codeänderung nötig. |

### E2E-Tests (primärer Funktionsnachweis)

Keine neuen E2E-Tests erforderlich: Die Anforderung berührt keinen neuen oder geänderten Benutzerfluss — sie härtet einen bestehenden Playwright-Test, der selbst der E2E-Nachweis des Baum-Collapse-Verhaltens ist. Die Produktfunktionalität (initial zugeklappte Endpunktordner) bleibt unverändert; es gibt nichts Neues, das über die UI nachgewiesen werden müsste.

Angepasste bestehende E2E-Tests:

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| `TreeCollapseTests.EndpointGroupInitiallyCollapsed` | Härtung des Reload-/Expand-Ablaufs (siehe oben) — bleibt der E2E-Nachweis „Ordner initial zugeklappt". |
| `TreeCollapseTests.ClickEndpointGroupName_TogglesCollapse`, `ClickEndpointGroupChevron_TogglesCollapse` | Gleiche Klick-Härtung, Assertions unverändert. |

## Akzeptanzszenarien

| # | Szenario | Auslöser | Ausgangszustand | Schritte | Sichtbare Wirkung | Nachweis |
|---|----------|----------|------------------|----------|-------------------|----------|
| 1 | Happy Path: gehärteter Testablauf | Lokaler/CI-Testlauf der OS-Lane | Frische Test-DB (`TestDatabaseSeeder.ResetAsync` — Systemgruppe/-anwendung existiert, kein Ordner, kein Expand-Zustand, keine Sitzung) | Ordner + Endpunkt anlegen → Badge-Warte → Speichern → Badge-Hidden-Warte → `ReloadAsync` → `ExpandSystemGroupAsync` → `ExpandSystemApplicationAsync` → Chevron-Assertion | Test besteht; die Ordnerzeile „Initial-Collapsed-Ordner" zeigt `sz-icon-chevron-right` | `dotnet test -- --filter "FullyQualifiedName~TreeCollapseTests"` grün; OS-Lane 94/94 grün |
| 2 | Verlorener Klick während Reconnect (Restfluctuation) | CI-Lauf mit langsamem SignalR-Reconnect nach Reload | Wie Szenario 1; erster App-Expand-Klick erreicht den Server nicht | Helper erkennt fehlendes `chevron-down`, klickt erneut (bis 3×) | Test besteht trotz Event-Verlust statt 30-s-Timeout | Struktur des Helpers + CI-Beobachtung; nicht deterministisch lokal provozierbar |
| 3 | Verfrühter Reload verhindert | Speichern-Write langsamer als Reload (CI-Last) | `_isDirty` aktiv, `beforeunload`-Guard registriert | Badge-Hidden-Warte blockiert `ReloadAsync` bis `OnSaveSuccessAsync` (Write + Guard-Disable) abgeschlossen ist | Reload findet garantiert ohne Guard-Blockade statt | Code-Inspektion + lokaler Lauf; Guard-Pfad muss nicht separat provoziert werden |
| 4 | Restfehlschlag → Einmal-Retry | Ein Fehlschlag trotz Härtung in der CI | `[Flaky]`-Trait gesetzt (Voraussetzung aus dieser Umsetzung) | `os-interface-tests`-Job: `Test OS interface` fail → `Retry flaky tests once` filtert `(Category=Flaky)&(FQN~EndpointGroupInitiallyCollapsed)` → Re-Run | `test-results-os-interface-*-flaky-retry.trx` entsteht; recovered → `::notice::`, persistent → `::warning::`; Job bleibt nicht-blockierend | CI-Artefakte + Job-Log beim nächsten Fehlschlag; Mechanismus durch Workflow-Definition gesichert |
| 5 | Konventionskonformität | Regulärer Testlauf | `[Flaky]` auf Methode, `[OsInterface]` auf Klasse | `FlakyConventionTests` scannt Assembly; `TraitParityTests` prüft Parität | Beide Meta-Tests grün; `--filter "Category=Flaky"` listet genau `EndpointGroupInitiallyCollapsed` | `dotnet test -- --filter "Category!=OsInterface"` grün; `dotnet test --list-tests -- --filter "Category=Flaky"` |
| 6 | Stabilität über Beobachtungsfenster | Zeitablauf / weitere CI-Läufe | Issue #112 offen oder via `Closes #112` bei Promotion geschlossen | Analyzer wertet die TRX-Historie der nächsten Läufe aus | Bei ≥ 5 Läufen ohne Fehlschlag: `[Flaky]` entfernen; bei Re-Flake: „Bekannte Fehlerbilder"-Eintrag + neuer Befund | Analyzer-Report im `os-interface-tests`-Job; Follow-up-Commit |

## Offene Punkte

Keine — die drei offenen Fragen der Anforderung sind geklärt bzw. in Festlegungen überführt:

- **Konkretes Fehlerbild:** durch TRX-Auswertung ermittelt (Run 45076613896: 30-s-Timeout auf `SystemAppRow`-`.sz-tree-item-btn` an `appBtn.ClickAsync()`; verlorener Gruppen-Chevron-Klick auf Code-Stand `0095a17`; bereits durch `b706a89` adressiert).
- **`beforeunload`-Dialogverhalten:** technisch nicht final verifiziert, aber durch die Speicher-Abschluss-Warte (Badge hidden ⇒ Guard deaktiviert ⇒ Reload ungehindert) vollständig mitigiert — eine Klärung der Browser-Semantik ist für den Fix nicht mehr nötig.
- **Geschwistertests:** werden mitgehärtet (Entscheidung oben), ohne `[Flaky]`.
