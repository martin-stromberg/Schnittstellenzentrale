# Tasks: Quarantäne `TreeCollapseTests.EndpointGroupInitiallyCollapsed`

| # | Bereich | Aufgabe | Status | Testnachweis |
|---|---------|---------|--------|--------------|
| 1 | Analyse | Fehlerbild des Fehlschlags aus CI-Artefakten in Issue #112 dokumentieren (Befund liegt vor: Run 45076613896, 30-s-Timeout auf `SystemAppRow`-`.sz-tree-item-btn` an `appBtn.ClickAsync()`, verlorener Gruppen-Chevron-Klick nach `ReloadAsync` auf Code-Stand `0095a17`; optional Trace `TreeCollapseTests-221833010.zip` via `npx playwright show-trace` sichten) | Offen | — |
| 2 | Testhärtung | Hilfsmethode `ExpandSystemApplicationAsync` in `TreeCollapseTests.cs` ergänzen: verifizierter Expand der Systemanwendungszeile — bis zu 3 Versuche, je 5-s-`WaitForAsync` auf `SystemAppRow`-`sz-icon-chevron-down`, sonst `appBtn.ClickAsync()`; finaler `WaitForAsync()` mit Default-Timeout (Muster: `ExpandSystemGroupAsync`) | Offen | — |
| 3 | Testhärtung | `EndpointGroupInitiallyCollapsed`: nach `FillAsync` des Pfad-Felds Sichtbarkeit von `.sz-endpoint-dirty-badge` abwarten; nach „Speichern"-Klick Verschwinden des Badges sowie Abwesenheit von `.sz-alert-danger`/`.sz-error` vor `Page.ReloadAsync()` abwarten | Offen | — |
| 4 | Testhärtung | `EndpointGroupInitiallyCollapsed`: Blindklick `appBtn.ClickAsync()` (Zeile 226) durch `ExpandSystemApplicationAsync()` ersetzen; Assertion-Timeout auf die Ordnerzeile (`sz-icon-chevron-right`) erhöhen (z. B. 15 s) | Offen | — |
| 5 | Markierung | `[Flaky]` auf Methodenebene von `EndpointGroupInitiallyCollapsed` setzen (über `[Fact]`; `OsInterface` vererbt von der Klasse) | Offen | — |
| 6 | Testhärtung | `ClickEndpointGroupName_TogglesCollapse`: Blindklick `appBtn.ClickAsync()` (Zeile 160) durch `ExpandSystemApplicationAsync()` ersetzen | Offen | — |
| 7 | Testhärtung | `ClickEndpointGroupChevron_TogglesCollapse`: Blindklick `appBtn.ClickAsync()` (Zeile 189) durch `ExpandSystemApplicationAsync()` ersetzen | Offen | — |
| 8 | Verifikation | Lokale Lanes ausführen: `dotnet test -- --filter "FullyQualifiedName~TreeCollapseTests"`, danach komplette OS-Lane (`Category=OsInterface`, Env `CI=true`) und reguläre Lane (`Category!=OsInterface`, inkl. `FlakyConventionTests`/`TraitParityTests`); `site/screenshots/*.png`-Neuschreibungen nach dem OS-Lauf per `git checkout` zurücksetzen | Offen | — |
| 9 | Abnahme | Commit mit `Closes #112` pushen, PR erstellen, PR-Checks per `gh pr checks --watch --fail-fast` überwachen | Offen | — |
| 10 | Beobachtung | Analyzer-Lage über die nächsten CI-Läufe prüfen; bei ≥ 5 stabilen Primär-Läufen `[Flaky]` entfernen (Follow-up-Commit); bei erneutem Fehlschlag Fehlerbild unter „Bekannte Fehlerbilder" in `docs/help/stabilitaet/os-interface-tests.md` ergänzen | Offen | — |
