## Fachliche Zusammenfassung
Die bestehende CI-Qualitaetssicherung wird nicht funktional erweitert, sondern durch zusaetzliche Tests stabilisiert, damit das bereits eingefuehrte Coverage-Gate (`>= 70 %` Line Coverage) zuverlaessig erfuellt wird. Ausloeser ist ein fehlschlagender `build-and-test`-Lauf mit 69 % gemessener Line-Coverage trotz Einbezug der Playwright-E2E-Tests. Fachlich ist damit eine gezielte Schliessung von Testluecken in bislang un- oder unterabgedeckten Blazor-Komponenten erforderlich. Die Schwelle selbst bleibt unveraendert; die Anforderung adressiert ausschliesslich die Testabdeckung.

## Betroffene Klassen und Komponenten
- **Datenmodellklassen**
  - Voraussichtlich keine Aenderungen an Domaenenmodellen erforderlich (Annahme auf Basis der Anforderung).
- **Logikklassen / Services**
  - Voraussichtlich keine produktiven Service-Aenderungen erforderlich.
  - Indirekt betroffen: Testinfrastruktur in `Schnittstellenzentrale.Tests` (z. B. vorhandene Mocks/Fixtures fuer UI-Tests).
- **Interfaces**
  - Keine neuen produktiven Interfaces ableitbar.
  - Bestehende Interfaces koennen in Tests gemockt werden (z. B. `IApplicationApiClient`, `IApplicationService`), falls fuer zusaetzliche bUnit-Tests noetig.
- **Enums**
  - Keine neuen Enums ableitbar.
- **UI-Komponenten / Controller**
  - Schwerpunkt auf Blazor-Komponenten unter `Schnittstellenzentrale.Components.Shared.*`, insbesondere Dialog-/Editor-Komponenten mit 0 % Coverage laut Aufgabenbeschreibung.
  - Konkrete Zielkomponenten sind erst nach Auswertung des Artifacts `coverage-report-pr` belastbar festzulegen.
- **Tests**
  - Erweiterung bestehender oder neue bUnit-Testklassen im Muster `*Tests` unter `src/Schnittstellenzentrale.Tests/Components/`.
  - Erweiterung bestehender oder neue Playwright-Testklassen im Muster `*Tests` unter `src/Schnittstellenzentrale.Tests/Playwright/`.
  - Verifikation ueber `dotnet test Schnittstellenzentrale.slnx --collect:"XPlat Code Coverage"`.

## Implementierungsansatz
1. Coverage-Luecken aus dem CI-Artifact `coverage-report-pr` priorisieren (insbesondere 0 %-Komponenten).
2. Pro betroffener Komponente entscheiden, ob bUnit (komponentenisoliert) oder Playwright (End-to-End/UI-Flow) die robustere Abdeckung liefert.
3. Tests entlang bestehender Testkonventionen ergaenzen:
   - bUnit-Komponententests als `ComponentNameTests`.
   - E2E-Szenarien als Playwright-`*Tests` auf Basis der vorhandenen `Playwright`-Infrastruktur.
4. Wiederholt lokal mit `dotnet test Schnittstellenzentrale.slnx --collect:"XPlat Code Coverage"` pruefen, bis die 70-%-Schwelle ueberschritten ist.
5. Relevanter Erweiterungspunkt in CI ist das vorhandene `build-and-test`-Gate in `pr-staging-ci.yml`/`staging-ci.yml` (laut Anforderung aus PR #62); dort ist keine Schwellenanpassung vorgesehen, sondern nur gruener Lauf durch hoehere Testabdeckung.

## Konfiguration
- Kein neuer Konfigurationsbedarf fuer Endanwender.
- CI-seitig bleibt die Coverage-Schwelle als feste Qualitaetsvorgabe (`70 %`) bestehen.
- Optional intern: Priorisierungsregel fuer kuenftige Testergaenzungen (z. B. zuerst 0 %-Komponenten), falls teamseitig gewuenscht.

## Offene Fragen
1. Welche konkreten Komponenten sind im aktuellen `coverage-report-pr` die Top-Kandidaten (exakte Liste inkl. aktueller Line-Coverage)?
2. Soll die Zielerreichung ausschliesslich ueber zusaetzliche Tests erfolgen, oder sind parallel kleine Refactorings zur Testbarkeit der Komponenten erlaubt?
3. Gibt es eine Praeferenzreihenfolge zwischen bUnit und Playwright bei Dialog-/Editor-Komponenten (z. B. zuerst bUnit, E2E nur fuer kritische Flows)?
4. Muss nur die globale 70-%-Schwelle erfuellt werden, oder gibt es zusaetzliche Mindestwerte pro Assembly/Namespace?
5. Sind Laufzeit-/Stabilitaetsgrenzen fuer zusaetzliche Playwright-Tests in CI definiert (um Flakiness und Build-Dauer zu begrenzen)?
6. Hinweis zum Projektkontext: Die in der Arbeitsanweisung referenzierte Datei `docs/features.md` war im aktuellen Stand nicht vorhanden; falls sie in einem anderen Branch/Commit existiert, sollte sie vor Umsetzung zusaetzlich abgeglichen werden.
