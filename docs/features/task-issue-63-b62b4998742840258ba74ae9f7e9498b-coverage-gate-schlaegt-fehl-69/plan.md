# Umsetzungsplan: Coverage Gate schlägt fehl

## Übersicht

Das Feature adressiert ausschließlich die Stabilisierung des bestehenden Coverage-Gates, nicht die Produktlogik selbst. Die Umsetzung fokussiert sich auf die gezielte Ergänzung fehlender `bUnit`-Tests für bislang unterabgedeckte Blazor-Komponenten und die Auswahl der wenigen, wirklich sinnvollen Playwright-Flows, falls zusätzliche End-to-End-Abdeckung für die 70-%-Schwelle oder darüber erforderlich ist. Die Mindestschwelle bleibt global bei 70 % Line Coverage, wird aber bewusst übererfüllt, sofern das mit gutem Code und ohne unnötige Breite machbar ist. Es gibt keine feste Komponenteliste; Kandidaten werden aus dem aktuellen Coverage-Report priorisiert, und wo das sinnvoll ist, wird bis nahe an 100 % Coverage gegangen.

## Designentscheidungen

| Komponente / Bereich | Gewählter Ansatz | Begründung |
|----------------------|-----------------|------------|
| Coverage-Ziel | Globale 70-%-Schwelle plus Bestreben auf möglichst hohe Abdeckung, auch über 100 % bei gutem Aufwand | Die Anforderung definiert die globale Schwelle, aber schließt höhere Abdeckung nicht aus. Da keine Liste vorliegt und die Coverage-Lücke bisher unklar ist, ist der pragmatische Ansatz: so viel wie möglich, aber sinnvoll und lesbar. |
| Teststrategie | Vorrangig `bUnit`; `Playwright` nur als gezieltes Ergänzungstool | Die klare Priorität aus den Antworten lautet `bUnit` zuerst. Für Blazor-Komponenten ist das die schnellste, robusteste und fokussierteste Methode, um UI-, State- und Dialogpfade mit geringem Aufwand abzudecken. |
| Refactoring-Politik | Kleine, gut begründete Refactorings sind erlaubt, aber keine umfangreichen Umbauten | Die Antwort erlaubt Refactoring, sofern der Code dadurch besser und schlanker wird; es soll aber kein großer Rework entstehen. Fokus liegt auf einfache Testbarkeitsergänzungen ohne Mehr-Code. |
| Priorisierung | Coverage-Report als primäre Quelle; keine feste vorab definierte Komponenteliste | Es wurde ausdrücklich keine Liste übergeben. Deshalb wird die Reihenfolge nach den tatsächlich gemessenen 0-%- und Niedrigabdeckungsstellen bestimmt. |
| Playwright-Kriterien | Keine festen CI-Limits vorgegeben; Laufzeit/Stabilität müssen empirisch geprüft werden | Der Punkt war ausdrücklich geklärt: Es gibt keine feste Vorgabe, sondern die zusätzliche Playwright-Abdeckung muss im realen Lauf ausprobiert und im CI-Verhalten bewertet werden. |

## Programmabläufe

### Coverage-Lücken priorisieren und schließen

1. Das aktuelle Coverage-Artifact bzw. der lokale Lauf wird als Ausgangspunkt verwendet, um Komponenten mit 0 % bzw. deutlich schlechter Abdeckung zu identifizieren.
2. Die betreffenden Komponenten werden nach ihrer Relevanz für UI-/State-/Dialoglogik eingeordnet; `bUnit` hat dabei Priorität.
3. Für jede Lücke wird entschieden, ob sie mit einem kompakten `bUnit`-Test oder nur mit einem gezielten Playwright-Flow abgedeckt wird.
4. Die resultierenden Testergänzungen werden iterativ mit `dotnet test Schnittstellenzentrale.slnx --collect:"XPlat Code Coverage"` geprüft, bis die globale 70-%-Schwelle erreicht ist und möglichst weiter erhöht wird.

Beteiligte Klassen/Komponenten: `AppShell`, `EnvironmentSelector`, `ApplicationContentView`, `MainLayoutTests`, `ApplicationContentViewTests`, `Playwright`-Testinfrastruktur.

### `bUnit`-Absicherung isolierter UI-Logik

1. Für jede relevante Blazor-Komponente wird ein möglichst kleiner `bUnit`-Test ergänzt, der Rendering, Events, Fehlerpfade und Dialogzustände prüft.
2. Abhängigkeiten wie `IApplicationApiClient`, `IApplicationService`, `IActiveEnvironmentService` und `ISystemEnvironmentRepository` werden über Mocks bzw. Test-Factory-Methoden bereitgestellt.
3. Erwartete UI-Ausgaben, `localStorage`-Interaktionen, Lade- und Fehlerpfade sowie Callback-/Dialogzustände werden als Assertions validiert.
4. Bestehende Testklassen werden erweitert, statt unnötig neue Schichten aufzubauen; das Ziel ist guter Code, nicht viel Code.

Beteiligte Klassen/Komponenten: `ApplicationContentViewTests`, `EnvironmentSelectorTests`, `MainLayoutTests`, `AppShellTests`, `TestMockFactory`.

### Playwright-Absicherung kritischer User Flows

1. Für die wichtigsten Benutzerabläufe, die mit `bUnit` allein nicht ausreichend abgedeckt sind, wird ein gezielter Playwright-Test ergänzt.
2. Die E2E-Tests nutzen die vorhandene Kestrel-/Playwright-Infrastruktur und prüfen echte Interaktion, Navigation und Dialogfluss.
3. Es werden nur die kritischsten und am stärksten wirkenden Flows aufgenommen, um Laufzeit und Flakiness im CI kontrolliert zu halten.
4. Die Ergebnisse werden zusammen mit dem Coverage-Lauf ausgewertet, damit die letzte Lücke zur 70-%-Grenze und darüber geschlossen wird.

Beteiligte Klassen/Komponenten: `PlaywrightServer`, `NavigationTests`, `SwaggerImportTests`, `ODataImportTests`, `EnvironmentManagementTests`, `LayoutSmokeTests`.

## Neue Klassen

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `CoverageGapComponentTests` | Testklasse | Ergänzt gezielt `bUnit`-Tests für bisher unterabgedeckte UI-, State- und Dialog-Pfade. |
| `CoverageGapPlaywrightTests` | Testklasse | Deckt kritische End-to-End-Flows ab, falls `bUnit` allein die Schwelle nicht ausreichend schließt. |
| `CoverageTestFactory` | Hilfsklasse / Testsupport | Zentralisiert gemeinsame Mock- und Fixture-Setups für Coverage-Lücken und spätere Test-Erweiterungen. |

## Änderungen an bestehenden Klassen

### `TestMockFactory` (Hilfsklasse)

- **Neue Eigenschaften:** `Keine`.
- **Neue Methoden:** `CreateCoverageScenarioDependencies` — liefert gemeinsame Mocks und Testdaten für UI-Fehlerszenarien und niedrige Coverage-Pfade.
- **Geänderte Methoden:** `CreateFakeLocalizer` — nur dann angepasst, wenn zusätzliche Komponenten lokalisierte Strings mit mehr Kontext oder Besonderheiten erwarten.
- **Neue Events:** `Keine`.
- **Neue Event-Handler:** `Keine`.

### `ApplicationContentViewTests` (Testklasse)

- **Neue Eigenschaften:** `Keine`.
- **Neue Methoden:** `OpenSwaggerImport_OnAdditionalErrorState_HandlesGracefully`, `OpenODataImport_OnAdditionalSuccessPath_HandlesGracefully` — ergänzt echte Branch- und Fehlerpfade.
- **Geänderte Methoden:** `Keine`.
- **Neue Events:** `Keine`.
- **Neue Event-Handler:** `Keine`.

### `EnvironmentSelectorTests` (Testklasse)

- **Neue Eigenschaften:** `Keine`.
- **Neue Methoden:** `RefreshAsync_WhenRepositoryReturnsEmptyList_HandlesGracefully`, `ApplySelectionAsync_WhenSelectionIsDeleted_ResetsState` — prüft leere, veraltete und Null-Fälle.
- **Geänderte Methoden:** `Keine`.
- **Neue Events:** `Keine`.
- **Neue Event-Handler:** `Keine`.

### `AppShellTests` / `MainLayoutTests` (Testklasse)

- **Neue Eigenschaften:** `Keine`.
- **Neue Methoden:** `OnAfterRender_WhenStorageStateIsIncomplete_ContinuesWithoutError`, `RestoreEnvironment_WhenModeChanges_UsesCurrentKey` — ergänzt Übergangs- und Restore-Pfade.
- **Geänderte Methoden:** `Keine`.
- **Neue Events:** `Keine`.
- **Neue Event-Handler:** `Keine`.

### Playwright-Testklassen (Testklasse)

- **Neue Eigenschaften:** `Keine`.
- **Neue Methoden:** `CriticalCoverageFlow_ImportsAndNavigatesSuccessfully` — prüft die wichtigsten realen UI-/Dialog- und Navigationspfade.
- **Geänderte Methoden:** `Keine`.
- **Neue Events:** `Keine`.
- **Neue Event-Handler:** `Keine`.

## Datenbankmigrationen

Keine.

## Validierungsregeln

Es gibt keine produktiven Datenmodell-Validierungen zu ändern; die relevante Überprüfung erfolgt durch UI- und State-Assertions in den Tests.

| Feld / Objekt | Regel | Fehlerfall |
|---------------|-------|------------|
| Komponenten-State | Rendern, Fehlerzustände und `localStorage`-Status müssen nach Interaktionen konsistent und reproduzierbar bleiben. | Test schlägt fehl, wenn der Zustandswechsel nicht korrekt umgesetzt wird. |
| Dialog-/Import-Flow | Erfolgs- und Fehlerpfad müssen getrennt validiert werden. | Ein fehlender oder falsch erwarteter Dialog-/Fehlerzustand wird nicht erfasst. |
| E2E-User-Flow | Kritische Interaktionen müssen auf einer realen Oberfläche erfolgreich und stabil durchlaufen werden. | UI-Flow ist flakey, nicht reproduzierbar oder unzureichend durch echte Benutzeraktionen abgebildet. |

## Konfigurationsänderungen

Keine.

## Seiteneffekte und Risiken

- **CI-Stabilität:** Zu viele oder zu breite Playwright-Tests können Laufzeit und Flakiness erhöhen; deshalb nur kritische Flows ergänzen, und die tatsächliche Gesamtwirkung empirisch prüfen.
- **Coverage-Driven-Tests:** Nur die Zahl ist nicht ausreichend; die Tests müssen echte Lücken und reale Zustandswechsel absichern.
- **Produktcode-Refactorings:** Kleine Refactorings sind erlaubt, aber nur als minimale Testbarkeitsergänzung; keine produktive Logikänderung, wenn sie nicht notwendig ist.
- **Unvollständige Funktionalität:** Die Anforderung ist rein testorientiert; ein zu aggressives Refactoring könnte unbeabsichtigt Produktverhalten verändern.

## Umsetzungsreihenfolge

1. **Coverage-Lücken aus dem Report priorisieren**
   - Voraussetzungen: vorhandenes `coverage-report-pr` bzw. lokaler Coverage-Run und Testbasis im Repository.
   - Beschreibung: Die konkreten Kandidaten werden anhand der 0-%- und Niedrigabdeckungs-Komponenten bestimmt; es gibt keine feste Liste.

2. **Betroffene Testklassen und Hilfsmittel identifizieren**
   - Voraussetzungen: bestehende `bUnit`-/Playwright-Teststruktur unter `src/Schnittstellenzentrale.Tests` sowie `TestMockFactory`.
   - Beschreibung: Es wird entschieden, welche vorhandenen Klassen erweitert werden und welche neuen Testklassen oder Hilfsfunktionen erforderlich sind.

3. **`bUnit`-Tests für die wichtigsten UI-/Dialog-/State-Pfade ergänzen**
   - Voraussetzungen: `IApplicationApiClient`, `IApplicationService`, `IActiveEnvironmentService`, `ISystemEnvironmentRepository`, `TestMockFactory`.
   - Beschreibung: Priorisierte Komponenten werden mit fokussierten `bUnit`-Tests abgesichert; kleine Refactorings werden nur dann eingebaut, wenn sie die Testbarkeit deutlich verbessern und wenig Code erzeugen.

4. **Playwright-Tests nur für kritische und signifikante Flows ergänzen**
   - Voraussetzungen: vorhandene Playwright-Infrastruktur, `PlaywrightServer`, relevante kritische UI-Flows.
   - Beschreibung: Es werden nur die wirklich entscheidenden End-to-End-Scenarios ergänzt, die zur 70-%-Grenze oder darüber beitragen und einen hohen Nutzerimpact haben.

5. **Lokalen Coverage-Lauf verifizieren**
   - Voraussetzungen: alle relevanten Tests und Hilfsmittel sind eingefügt; .NET-Umgebung installiert.
   - Beschreibung: `dotnet test Schnittstellenzentrale.slnx --collect:"XPlat Code Coverage"` wird wiederholt ausgeführt und die Ergebnisse interpretiert.

6. **Abschlussprüfung und CI-Stabilität**
   - Voraussetzungen: lokaler grüner Coverage-Lauf und keine ungewollten Produktänderungen.
   - Beschreibung: Der bestehende CI-Gate-Mechanismus bleibt unverändert; es wird bestätigt, dass der Lauf mit den zusätzlichen Tests stabil und grün bleibt.

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `CoverageGapComponentTests` | `src/Schnittstellenzentrale.Tests/Components/` | Zentrale `bUnit`-Klasse für bislang un- oder schwach abgedeckte UI-/Dialog-Pfade. |
| `CoverageGapPlaywrightTests` | `src/Schnittstellenzentrale.Tests/Playwright/` | Kritische, realitätsnahe E2E-Szenarien für die besten Coverage-Gewinne. |
| `CreateCoverageScenarioDependencies` | `TestMockFactory` | Gemeinsame Mock-/Fixture-Erzeugung für Wiederverwendung in mehreren Komponenten-Tests. |

### Betroffene bestehende Tests

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| `ApplicationContentViewTests` | Ergänzung fehlender Branch- und Dialogfehlerpfade. |
| `EnvironmentSelectorTests` | Zusätzliche Absicherung für leere Listen, fehlerhafte Auswahl und Null-Fälle. |
| `AppShellTests` | Ergänzung der Restore- und State-Wechsel-Pfade. |
| `MainLayoutTests` | Abdeckung von Navigation, Storage- und Initialisierungszuständen. |
| `NavigationTests`, `SwaggerImportTests`, `ODataImportTests` | Für kritische E2E-Flow-Absicherung bei Bedarf erweitert. |

### E2E-Tests (Pflicht)

| Szenario | Testdatei / Testklasse | Abgedecktes Akzeptanzkriterium |
|----------|------------------------|-------------------------------|
| `CoverageGap_ImportAndNavigateFlow` | Playwright-Testklasse | Ein kritischer Import-/Navigationslauf durchläuft die Oberfläche fehlerfrei und stabil. |
| `CoverageGap_EnvironmentRestoreFlow` | Playwright-Testklasse | Die Umgebung wird nach erneutem Laden sauber wiederhergestellt und die UI bleibt konsistent. |
| `CoverageGap_DialogAndStateFlow` | Playwright-Testklasse | Dialog-Öffnung, Fehlerpfad und Zustandswechsel funktionieren real gemäß Nutzerinteraktion. |

Welche bestehenden E2E-Tests müssen angepasst werden?

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| `LayoutSmokeTests` | Verifiziert die Layout-Stabilität weiterhin; zusätzliche UI-Elemente dürfen das Layout nicht beschädigen. |
| `NavigationTests` | Bei erforderlichen Coverage-Verbesserungen kann die E2E-Abdeckung um zusätzliche Assertions für kritische Pfade erweitert werden. |

## Offene Punkte

Keine. Die im Anforderungskorpus beantworteten Punkte wurden in die vorgenannten Entscheidungen und Abläufe eingearbeitet; keine weiteren offenen technischen oder fachlichen Fragen verbleiben. Die Datei `docs/features.md` war im aktuellen Stand nicht vorhanden und wird für diesen Plan nicht als Voraussetzung angesehen, da die vorhandenen Feature-Unterlagen im betroffenen Featureordner bereits ausreichen.
