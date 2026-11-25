# Bestandsaufnahme: Quarantäne `TreeCollapseTests.EndpointGroupInitiallyCollapsed`

Analysiert wurden der Playwright-Test `Schnittstellenzentrale.Tests.Playwright.TreeCollapseTests.EndpointGroupInitiallyCollapsed`, seine Testinfrastruktur (Basisklasse, Server-Fixture, Trait-Attribute) sowie die vom Test durchlaufenen UI-Komponenten — bezogen auf die Quarantäne-Anforderung aus Issue #112 (Analyzer-Befund: 1 Fehlschlag in 5 CI-Ausführungen = 20 % Fehlrate).

## Zusammenfassung

- Der betroffene Test existiert in `TreeCollapseTests.cs` (Zeilen 202–230) und enthält den im Issue beschriebenen Blindklick `appBtn.ClickAsync()` (Zeile 226) direkt nach `Page.ReloadAsync()` (Zeile 222). Die Geschwistertests `ClickEndpointGroupName_TogglesCollapse` (Zeile 160) und `ClickEndpointGroupChevron_TogglesCollapse` (Zeile 189) enthalten denselben Blindklick, jedoch ohne vorheriges `ReloadAsync`.
- Der bereits gehärtete Helfer `ExpandSystemGroupAsync` (Zeilen 118–138) existiert mit verifiziertem Expand (bis zu 3 Versuche, Sichtbarkeitsprüfung der Zielzeile vor jedem Klick) und wird von drei Tests genutzt. Er klappt allerdings die **Gruppenzeile** (`.collapsible-section`-Chevron) auf, nicht die Anwendungszeile — der Expand der Anwendungszeile ist überall Blindklick.
- Das Attribut `FlakyAttribute` (`Infrastructure/Testing/FlakyAttribute.cs`) existiert und ist **aktuell nirgends in `src/` verwendet** — `EndpointGroupInitiallyCollapsed` trägt (wie alle Methoden der Klasse) nur das klassenweite `[OsInterface]`. Die Kombinationsregel ist durch `FlakyConventionTests` abgesichert.
- Expand-Zustände (`_expandedGroupIds`, `_expandedApplicationIds`, `_expandedEndpointGroupIds` in `ApplicationGroupTree.razor`; `_expanded` in `CollapsibleSection.razor`) sind komponenteninterner Speicher ohne Persistenz — nach `Page.ReloadAsync()` ist fachlich alles zugeklappt (bestätigt Hypothese-3-Widerlegung der Anforderung).
- Der Chevron einer Endpunktgruppenzeile wird in `RenderEndpointGroup` (`ApplicationGroupTree.razor` Zeilen 462–464) **unbedingt** gerendert — auch für leere Ordner. Der im Test angelegte Endpunkt landet ohnehin auf Anwendungsebene (`HandleCreateEndpointRequested` mit `Group = null`, `WorkspacesLayout.razor` Zeilen 267–310).
- `EndpointPage.razor` hat bei `_isDirty` einen `beforeunload`-Guard (`endpoint-page.js` `enableBeforeUnloadGuard`), der nach erfolgreichem Speichern via `OnSaveSuccessAsync` → `DisableBeforeUnloadGuardAsync` aufgehoben wird; sichtbares Signal ist das Verschwinden des Dirty-Badges `.sz-endpoint-dirty-badge` (Zeilen 17–19).
- CI-Infrastruktur komplett vorhanden: best-effort-Job `os-interface-tests` in `pr-staging-ci.yml`/`staging-ci.yml` mit Einmal-Retry (`Retry-FlakyTests.ps1`, `if: failure()`), Analyzer (`Analyze-FlakyTestResults.ps1`) und deduplizierter Quarantäne-Issue-Erzeugung (`New-FlakyQuarantineIssue.ps1`). Trace-ZIPs werden pro Test unter `playwright-traces/` geschrieben (gitignored, CI-Artefakt).

**Test-Ausgangszustand:** Beide projektüblichen Testlanes lokal ausgeführt und vollständig grün — reguläre Lane (`Category!=OsInterface`): 641/641 bestanden; OS-Schnittstellen-Lane (`Category=OsInterface`): 94/94 bestanden, darunter `EndpointGroupInitiallyCollapsed` (12,1 s). Keine bestehenden Testfehler nachgewiesen. Nachweis und Details: [Tests](inventory/tests.md).

## Details

- [Datenmodell](inventory/models.md) — `EndpointGroup`, `Endpoint`, `Application`, `ApplicationGroup` (für den Testfluss relevante Eigenschaften)
- [Logik / UI-Komponenten](inventory/logic.md) — `ApplicationGroupTree`, `CollapsibleSection`, `EndpointPage`, `WorkspacesLayout`, `WorkspacesSidebar`, `ApplicationContextMenu`, `endpoint-page.js`, `SystemEntryInitializer`
- [Enums und Kategoriekonstanten](inventory/enums.md) — `TestCategories`, `HttpMethod`, `BodyMode`
- [Interfaces](inventory/interfaces.md) — `IApplicationApiClient`, `Xunit.v3.ITraitAttribute`
- [Testinfrastruktur und CI](inventory/test-infrastructure.md) — `PlaywrightServer`, `PlaywrightTestBase`, `TestDatabaseSeeder`, Trait-Attribute/Meta-Tests, CI-Workflows, Analyzer-/Retry-Skripte
- [Tests](inventory/tests.md) — Test-Ausgangszustand (Nachweis), Testklassen und Hilfsmethoden
