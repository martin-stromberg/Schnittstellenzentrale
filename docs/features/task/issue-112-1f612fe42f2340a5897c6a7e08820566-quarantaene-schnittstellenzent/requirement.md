# Quarantäne: `TreeCollapseTests.EndpointGroupInitiallyCollapsed`

**Quelle:** GitHub-Issue #112 (automatisch angelegt durch `scripts/New-FlakyQuarantineIssue.ps1` auf Basis eines Analyzer-Befunds aus `scripts/Analyze-FlakyTestResults.ps1`).

## Fachliche Zusammenfassung

Der Playwright-Test `Schnittstellenzentrale.Tests.Playwright.TreeCollapseTests.EndpointGroupInitiallyCollapsed` ist vom Flake-Analyzer als Quarantäne-Kandidat eingestuft (1 Fehlschlag in 5 Ausführungen = 20 % Fehlrate, exakt am Schwellwert). Zu bearbeiten ist die Teststabilität des Reload-/Expand-Ablaufs in `src/Schnittstellenzentrale.Tests/Playwright/TreeCollapseTests.cs`: Das konkrete Fehlerbild des einen Fehlschlags ist aus den CI-Artefakten (TRX-`<Output>`-Block, `playwright-traces-*`-ZIP) zu ermitteln, die verbliebenen Blind-Klick- und Warte-Lücken im Test sind nach dem Vorbild des bereits gehärteten `ExpandSystemGroupAsync` zu schließen, und der Test ist regelkonform mit `[Flaky]` zu markieren. Ein Produktfehler ist nicht Gegenstand, sofern die Artefakt-Analyse keinen fachlichen Defekt offenbart.

## Fachlicher Scope

- **Verarbeitungseinheit:** Ein Quarantäne-Vorgang für genau einen OS-Schnittstellen-Test (`EndpointGroupInitiallyCollapsed`): Fehlerbild-Analyse des dokumentierten Fehlschlags, Behebung der identifizierten Teststabilisierungslücke(n), `Flaky`-Markierung und Wiederbeobachtung über das nächste Analyzer-Auswertefenster.
- **Auslösende Ereignisse:** Automatischer Analyzer-Befund (Fehlrate ≥ 20 % bei ≥ 5 Ausführungen) in der CI, der das deduplizierte Quarantäne-Issue erzeugt hat; fachlicher Auslöser im Test selbst ist das `Page.ReloadAsync()` mit anschließendem Expand der Anwendungszeile.
- **Erwartete Wirkung:** Der Test schlägt in der best-effort-Lane `os-interface-tests` nicht mehr wegen verlorener UI-Events oder verfrühter Reloads fehl; ein etwaiger Restfehlschlag wird durch die `Flaky`-Markierung genau einmal automatisch wiederholt (`scripts/Retry-FlakyTests.ps1`) und bleibt sichtbar statt blockierend. Das dokumentierte Fehlerbild ist im Issue bzw. bei Wiederholung unter „Bekannte Fehlerbilder" in `docs/help/stabilitaet/os-interface-tests.md` festgehalten.
- **Umgang mit fehlenden Objekten:**
  - CI-Artefakte des fehlgeschlagenen Laufs (`test-results-os-interface-*`, `playwright-traces-*`): sind über `gh run download` zu beschaffen; bei abgelaufener Artefakt-Aufbewahrung ist die Nichtverfügbarkeit zu dokumentieren und die Analyse auf den Analyzer-Befund sowie die Code-Analyse der Hypothesen zu stützen (verwerfen statt erfinden).
  - Trace-ZIP: wird pro Testlauf automatisch unter `playwright-traces/` erzeugt (`PlaywrightTestBase`, Dateiname `{TestName}-HHmmssfff.zip`) — wiederverwenden, nicht neu erzeugen.
  - `Flaky`-Markierung: vorhandenes Attribut `Infrastructure/Testing/FlakyAttribute.cs` wiederverwenden (Methodenebene auf `[Fact]`); die Klasse trägt bereits `[OsInterface]`, die Kombinationsregel ist durch `FlakyConventionTests` abgesichert.
- **Scope-Ausschlüsse:**
  - Keine Änderung am Produktverhalten des Baums (Auf-/Zuklapp-Logik, SignalR-Hub, API), solange die Artefakt-Analyse keinen fachlichen Defekt zeigt. Der Code-Stand bestätigt bereits: Der Expand-Zustand (`_expandedEndpointGroupIds`, `_expandedApplicationIds`, `_expandedGroupIds` in `ApplicationGroupTree.razor`, `_expanded` in `CollapsibleSection.razor`) ist rein komponenteninterner Speicher ohne Persistenz — nach `Page.ReloadAsync()` ist fachlich korrekt alles zugeklappt, Hypothese 3 (persistierter Expand-Zustand) ist damit auf Code-Ebene entkräftet.
  - Keine Umbaumaßnahme an der Quarantäne-/Retry-Infrastruktur (`Analyze-FlakyTestResults.ps1`, `Retry-FlakyTests.ps1`, Workflows).
  - Keine Ausweitung der `Flaky`-Markierung auf Geschwistertests ohne eigenen dokumentierten Befund; deren identisches Klick-Muster darf aber im selben Zug mitgehärtet werden (siehe Implementierungsansatz).

## Betroffene Klassen und Komponenten

- **Tests (primär):**
  - `src/Schnittstellenzentrale.Tests/Playwright/TreeCollapseTests.cs` — Testmethode `EndpointGroupInitiallyCollapsed` (Zeilen 203–230); bestehender Helper `ExpandSystemGroupAsync` (Zeilen 118–138) und Locator `SystemAppRow` (Zeilen 140–141); optional gleiche Härtung in `ClickEndpointGroupName_TogglesCollapse` (Blindklick Zeile 160) und `ClickEndpointGroupChevron_TogglesCollapse` (Blindklick Zeile 189).
  - `src/Schnittstellenzentrale.Tests/Infrastructure/Testing/FlakyAttribute.cs` — vorhandenes Attribut, an der Testmethode anzubringen (keine Änderung am Attribut selbst).
  - `src/Schnittstellenzentrale.Tests/Infrastructure/Testing/FlakyConventionTests.cs` — Meta-Test, der die `Flaky`-nur-mit-`OsInterface`-Regel erzwingt (läuft mit, nicht zu ändern).
- **Referenzierte Produktartefakte (Analyse-Kontext, voraussichtlich unverändert):**
  - `src/Schnittstellenzentrale/Components/Shared/ApplicationGroupTree.razor` — Expand-Zustände (Zeilen 85–87), `ToggleApplicationExpanded`/`SelectAndExpandApplication` (Zeilen 263–284), `RenderEndpointGroup` mit unbedingt gerendertem Chevron (Zeilen 460–472).
  - `src/Schnittstellenzentrale/Components/Shared/CollapsibleSection.razor` — komponentenlokaler `_expanded`-Zustand der Gruppenzeilen.
  - `src/Schnittstellenzentrale/Components/Shared/EndpointPage.razor` — `SaveAsync`/`OnSaveSuccessAsync` (Zeilen 386–472) mit `.sz-endpoint-dirty-badge` (Zeilen 17–19) als sichtbarem Speicher-Abschlusssignal und `beforeunload`-Guard bei `_isDirty`.
  - `src/Schnittstellenzentrale/Components/Layout/WorkspacesLayout.razor` — `HandleCreateEndpointRequested` (Zeilen 267–310): „Endpunkt anlegen" über das Anwendungs-Kontextmenü legt den Endpunkt auf Anwendungsebene an (`EndpointGroupId = args.Group?.Id` = `null`), nicht im Ordner.
- **Dokumentation:** `docs/help/stabilitaet/os-interface-tests.md` (Abschnitt „Bekannte Fehlerbilder" nur bei wiederholtem Fehlschlag).

## Implementierungsansatz

1. **Fehlerbild sichern (Analyse-Schritt vor Fix):** TRX des fehlgeschlagenen Laufs (`<Output>`-Block enthält `ITestOutputHelper`-Diagnosen aus `PlaywrightTestBase`) und das zugehörige `playwright-traces-*`-Artefakt aus dem CI-Lauf laden; `npx playwright show-trace <zip>` auswerten. Erwartung auf Code-Basis: Timeout der Assertion `Expect(groupRow.Locator(".sz-icon-chevron-right")).ToBeVisibleAsync()` (Zeile 229), weil die Ordnerzeile nie erschien — zu unterscheiden: Assertion-Timeout, Strict-Mode-Verletzung oder Navigations-/Reload-Fehler.
2. **Hypothese 1 — verifizierter Expand der Anwendungszeile:** Den Blindklick `appBtn.ClickAsync()` (Zeile 226) nach `Page.ReloadAsync()` durch ein verifizierendes Muster ersetzen — analog `ExpandSystemGroupAsync`: vor jedem Klick prüfen, ob die Zielzeile (Ordnerzeile bzw. `sz-icon-chevron-down` an der Anwendungszeile) bereits sichtbar ist, sonst klicken, begrenzte Wiederholung. Hintergrund: Während des SignalR-Reconnects verwirft Blazor Server UI-Events ohne aktiven Circuit; der Klick auf `.sz-tree-item-btn` löst `SelectAndExpandApplication` → `ToggleApplicationExpanded` → `ReloadApplicationDataAsync` (REST-Roundtrip) aus — geht er verloren, wird `_expandedApplicationIds` nie befüllt und die Ordnerzeile nie gerendert. Zu beachten: `Expect(...).ToBeVisibleAsync()` nutzt den Playwright-Assertion-Default (5 s), nicht `SetDefaultTimeout(30 s)` — die Wartebedingung im Helper sollte daher auf die Zielzeile selbst zeigen, nicht auf eine Folge-Assertion.
3. **Hypothese 2 — Speicher-Abschluss vor `Page.ReloadAsync()` abwarten:** Nach „Speichern" auf das Verschwinden des Dirty-Badges (`.sz-endpoint-dirty-badge`, `EndpointPage.razor` Zeilen 17–19) und Abwesenheit von `.sz-alert-danger` warten. Das schließt zwei Lücken: den REST-Write (`UpdateEndpointAsync`) und die Aufhebung des `beforeunload`-Guards (`DisableBeforeUnloadGuardAsync`), der einen Reload bei `_isDirty` blockieren kann. Code-Klarstellung zur Issue-Frage: Ein leerer Endpunktordner rendert den Chevron unbedingt (`ApplicationGroupTree.razor` Zeilen 462–463) — und der im Test angelegte Endpunkt landet ohnehin auf Anwendungsebene (Aufruf über das Anwendungs-Kontextmenü, `Group = null`); die Ordnerzeile existiert also unabhängig vom Endpunkt-Write.
4. **Hypothese 3 — entfällt:** Der Expand-Zustand ist komponenteninterner Speicher ohne Persistenz oder localStorage; nach Reload rendert jeder Ordner zugeklappt. Keine Assertions- oder Produktanpassung nötig.
5. **`[Flaky]` markieren:** `[Flaky]` auf Methodenebene über `[Fact]` setzen (Klasse trägt bereits `[OsInterface]`; der dokumentierte Analyzer-Befund erfüllt die Vergaberegel laut `docs/help/stabilitaet/os-interface-tests.md`). Dadurch greift der Einmal-Retry `scripts/Retry-FlakyTests.ps1` in der best-effort-Lane.
6. **Geschwister-Härtung (optional, gleiche Lücke):** Die identischen Blindklicks in `ClickEndpointGroupName_TogglesCollapse` und `ClickEndpointGroupChevron_TogglesCollapse` können denselben Verifizierungs-Helper nutzen; ohne `ReloadAsync` vor dem Klick ist ihr Risiko aber geringer — Entscheidung anhand des Fehlerbilds.
7. **Verifikation und Beobachtung:** Lokal `dotnet test -- --filter "Category=OsInterface"` (bzw. gezielt `FullyQualifiedName~TreeCollapseTests`), danach Analyzer-Lage über die nächsten CI-Läufe prüfen; bei anhaltender Stabilität Issue schließen, bei Wiederholung „Bekannte Fehlerbilder" in `docs/help/stabilitaet/os-interface-tests.md` ergänzen.

## Konfiguration

Keine. Analyzer-Schwellwerte bleiben unverändert (bereits kalibrierbar über Skript-Parameter, kein Teil dieser Aufgabe).

## Offene Fragen

1. **Konkretes Fehlerbild des einen Fehlschlags:** Timeout der `ToBeVisibleAsync`-Assertion auf die Ordnerzeile, Strict-Mode-Verletzung oder Reload-/Navigationsfehler — erst aus TRX-`<Output>` und Trace-ZIP des CI-Laufs zu beantworten (Artefakt-Verfügbarkeit prüfen; die Befundlage von 1 Fehlschlag in 5 Läufen ist dünn, ggf. bleibt die Ursache eine plausible, nicht bewiesene Hypothese).
2. **Playwright-Dialogverhalten bei `beforeunload`:** Ob ein durch `beforeunload` blockierter `Page.ReloadAsync()` ohne registrierten Dialog-Handler die Navigation verwirft (Reload bleibt aus) und ob dieses Szenario im Fehlerbild überhaupt auftritt — aus dem Trace zu verifizieren; die Warte-Abhärtung nach Punkt 2/3 schließt die Lücke unabhängig davon.
3. **Geschwistertests:** `ClickEndpointGroupName_TogglesCollapse` und `ClickEndpointGroupChevron_TogglesCollapse` teilen den Blindklick auf die Anwendungszeile, jedoch ohne vorheriges `ReloadAsync` — offen, ob sie im selben Zug mitgehärtet werden sollen oder nur bei eigenem Analyzer-Befund.
