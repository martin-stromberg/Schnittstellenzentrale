# Übersetzte Anforderung – Quarantäne: ODataEndpointExecutionTests (Issue #118)

## Ziel

Der Playwright-E2E-Test `Schnittstellenzentrale.Tests.Playwright.ODataEndpointExecutionTests.OdataApi_Registrieren_Importieren_UndAlleEndpunkteErfolgreichAusfuehren` wurde vom Flake-Analyzer (`scripts/Analyze-FlakyTestResults.ps1`) als Quarantäne-Kandidat eingestuft. Ziel ist es, die Ursache der erhöhten Fehlrate aufzuklären und den Test wieder in einen dauerhaft stabilen Zustand zu versetzen — entweder durch Behebung eines echten Produkt- oder Infrastrukturdefekts, durch Testhärtung oder durch eine begründete `[Flaky]`-Markierung.

## Kontext

- **Auslöser:** Fehlraten-Schwelle des Analyzers (Fehlrate ≥ 20 % bei ≥ 5 Ausführungen), nicht die Flake-Ereignis-Schwelle (≥ 2 Flake-Ereignisse).
- **Kennzahlen (Auswertungsfenster: letzte `MaxRuns=20` Primär-Läufe):** 9 Ausführungen, 2 Fehlschläge, 0 Flake-Ereignisse, Fehlrate 22,2 %.
- **Testgegenstand:** E2E-Ablauf über Playwright — die eigene OData-API wird als Anwendung `SZ-OData-E2E` registriert (Basis-URL `{BaseUrl}/odatav4`), OData-Metadaten werden über den „OData-Import"-Dialog importiert, danach werden drei importierte Endpunkte über die UI ausgeführt und jeweils 2xx-Status plus OData-JSON-Body assertiert: `Authenticate` (Response enthält `token`), `GET ApplicationGroups` und `GET Applications` (Response enthält `isSystem`).
- **Besonderheit des Befundes:** 0 Flake-Ereignisse bei 2 Fehlschlägen bedeutet, dass nach keinem Fehlschlag ein grüner Folgelauf beobachtet wurde. Das ist ein Hinweis auf möglicherweise aufeinanderfolgende Fehlschläge bzw. eine persistente Störung statt klassischem Flaky-Verhalten. Vor jeder `[Flaky]`-Markierung muss daher geklärt werden, ob ein echter Defekt vorliegt.
- **Aktueller Zustand:** Der Test ist derzeit nicht `[Flaky]`-markiert.
- **Relevante Artefakte (Orientierung):**
  - Testdatei: `src/Schnittstellenzentrale.Tests/Playwright/ODataEndpointExecutionTests.cs`
  - Analyzer: `scripts/Analyze-FlakyTestResults.ps1` (Fenster `MaxRuns=20`; Quarantäne bei ≥ 2 Flake-Events ODER Fehlrate ≥ 20 % bei ≥ 5 Ausführungen)
  - Stabilitätsdokumentation: `docs/help/stabilitaet/os-interface-tests.md` (enthält „Bekannte Fehlerbilder", Flaky-Vergaberegeln, Härtungsmuster)
- **CI-Einordnung:** Der Job `os-interface-tests` ist best-effort (`continue-on-error`); Fehlschläge dieser Lane blockieren keine Merges.

## Funktionale Anforderungen

1. **Fehlerbild sichern und konkretisieren:** Die TRX-/Trace-Artefakte der betroffenen `os-interface-tests`-Läufe sind auszuwerten. Zu benennen sind: die fehlschlagenden Runs (mit Run-Links), der jeweils fehlschlagende Schritt, die konkrete Fehlermeldung sowie ob die Fehlschläge aufeinanderfolgend auftraten. Diese Angaben sind im Issue bzw. den Ergebnisartefakten zu ergänzen.
2. **Ursache klassifizieren:** Es ist zu dokumentieren, ob ein dokumentierter Flake (instabiles Testverhalten ohne Produktdefekt), ein persistenter Defekt (Produkt- oder Infrastrukturfehler) oder ein vorübergehendes/behobenes Problem vorliegt.
3. **Ursache beheben oder Test härten:** Abhängig von der Klassifikation ist die gefundene Ursache zu beseitigen oder der Test gemäß den in `docs/help/stabilitaet/os-interface-tests.md` beschriebenen Mustern zu stabilisieren.
   - Konkreter Verdachtspunkt aus dem Issue: mehrfache blinde Doppel-Klicks auf Chevron-Toggles ohne Ergebnis-Warte — ein bereits dokumentierter Flake-Mechanismus (vgl. Issue #112). Das dort eingeführte Härtungsmuster `ExpandWithRetryAsync` aus `TreeCollapseTests` ist in diesem Test noch nicht angewendet.
   - Der Test ist ein langer Monolith mit ca. 30 UI-Interaktionen; eine Aufteilung ist optional denkbar, aber nicht zwingend.
4. **Entscheidungsregel zum Abschluss:**
   - Dokumentierter Flake: `[Flaky]`-Markierung — ausschließlich in Kombination mit `[OsInterface]`.
   - Persistente Störung: Ursache beheben, **kein** `[Flaky]`.
   - Anhaltend grüne Statistik ohne Befund: Issue schließen.
5. **Dokumentation neuer Fehlerbilder:** Falls ein bisher nicht dokumentiertes Fehlerbild identifiziert wird, ist ein Eintrag unter „Bekannte Fehlerbilder" in `docs/help/stabilitaet/os-interface-tests.md` zu ergänzen.

## Nicht-Ziele

- Keine `[Flaky]`-Markierung ohne vorherige, dokumentierte Befundklärung.
- Keine Änderung der Analyzer-Logik oder der Quarantäne-Schwellenwerte in `scripts/Analyze-FlakyTestResults.ps1`.
- Keine Änderung am CI-Charakter des `os-interface-tests`-Jobs (best-effort bleibt erhalten).
- Keine produktseitige Funktionserweiterung der Schnittstellenzentrale — es geht ausschließlich um Teststabilität bzw. um die Behebung eines ggf. dahinterliegenden Defekts.
- Keine generelle Refaktorierung der Playwright-Testsuite über den betroffenen Test hinaus.

## Randbedingungen

- Die reguläre Test-Lane (`dotnet test --filter "Category!=OsInterface"`) ist blockierend und darf nicht beeinträchtigt werden.
- Flaky-Vergaberegeln laut Stabilitätsdokumentation sind bindend: `[Flaky]` nur zusammen mit `[OsInterface]` und nur bei dokumentiertem Flake-Befund.
- Läuft die Maßnahme auf eine Teständerung hinaus, muss die Fehlrate im Analyzer-Fenster anschließend wieder unter die Quarantäne-Schwelle fallen bzw. der Test dauerhaft stabil sein.
- Da der `os-interface-tests`-Job `continue-on-error` ist, erfolgt die Beurteilung der Stabilität über die Analyzer-Auswertung, nicht über Merge-Blockierung.

## Akzeptanzkriterien

- Die fehlenden Angaben sind ergänzt: Run-Links der betroffenen Läufe, fehlschlagender Schritt, konkrete Fehlermeldung und Aussage zur Aufeinanderfolge der Fehlschläge.
- Die Ursache ist dokumentiert (Flake vs. persistent); bei einem neuen Fehlerbild existiert ein Eintrag unter „Bekannte Fehlerbilder" in `docs/help/stabilitaet/os-interface-tests.md`.
- Es wurde entweder ein Fix bzw. eine Testhärtung umgesetzt ODER eine `[Flaky]`-Markierung mit dokumentiertem Flake-Befund vorgenommen.
- Die Fehlrate im Analyzer-Fenster liegt wieder unter der Quarantäne-Schwelle bzw. der Test ist dauerhaft stabil — andernfalls bleibt der Befund dokumentiert und das Issue offen bzw. wird mit Begründung geschlossen.
