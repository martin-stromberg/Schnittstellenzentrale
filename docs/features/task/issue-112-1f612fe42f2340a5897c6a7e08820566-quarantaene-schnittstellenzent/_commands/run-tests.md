# Tests ausführen

Führe alle automatisierten Tests des Projekts aus, miss die Testabdeckung und melde Dateien ohne Tests.

**Ziel:** Regressionssicherheit nach der Implementierung prüfen — keine Code-Analyse, keine inhaltliche Bewertung, nur Testergebnisse und Abdeckungslücken.

---

## Schritt 1: Test-Runner ermitteln

Prüfe die vorhandenen Projektdateien und leite daraus den passenden Befehl ab:

| Datei vorhanden | Test-Befehl | Coverage-Befehl |
|-----------------|-------------|-----------------|
| `*.sln` oder `*.csproj` | `dotnet build && dotnet test --no-build` | `dotnet build && dotnet test --no-build --collect:"XPlat Code Coverage"` |
| `package.json` mit `scripts.test` | `npm test -- --passWithNoTests` | `npm test -- --passWithNoTests --coverage` |
| `pytest.ini`, `pyproject.toml`, `setup.cfg` oder `conftest.py` | `pytest -v` | `pytest -v --cov --cov-report=term-missing` |
| `pom.xml` | `mvn test -q` | `mvn test jacoco:report -q` (nur wenn Jacoco konfiguriert) |
| `build.gradle` oder `build.gradle.kts` | `./gradlew test` | `./gradlew test jacocoTestReport` |
| `Cargo.toml` | `cargo test` | `cargo tarpaulin --out Stdout` (nur wenn installiert) |
| `go.mod` | `go test ./...` | `go test -cover ./...` |

Ist kein Test-Runner erkennbar, gib im Ergebnis Status `Kein Test-Runner gefunden` aus und brich ab.

**Wichtig bei `dotnet`-Projekten:** Führe vor jedem `dotnet test` zwingend einen vorausgehenden `dotnet build` der gesamten Solution aus (nicht nur eines Einzelprojekts) und stelle sicher, dass dieser mit 0 Fehlern durchläuft, bevor `--no-build` verwendet wird. Ein übersprungener oder unvollständiger Build (z. B. nur ein Testprojekt statt der ganzen Solution, oder ein Build während noch ein `testhost`-Prozess Dateien blockiert) führt bei UI-Automatisierungstests (z. B. FlaUI/WPF-E2E-Tests) zu irreführenden Fehlern wie „Element wurde nicht gefunden" oder einem vermeintlich fehlenden „.NET Desktop Runtime", die wie Test-Flakiness aussehen, aber tatsächlich Build-Artefakte betreffen. Während der Testausführung dürfen keine Codeänderungen am Projekt vorgenommen werden.

## Schritt 2: Geplante E2E-Szenarien ermitteln

Falls `docs/features/{branchname}/plan.md` vorhanden ist, lies den Abschnitt `E2E-Tests`. Falls `docs/features/{branchname}/plan-check.md` vorhanden ist, lies zusätzlich den Abschnitt `E2E-Abdeckung`.

Ermittle daraus alle E2E-Szenarien, die als `Pflicht`, `Abgedeckt` oder anderweitig als notwendig beschrieben sind. Für Benutzerflüsse sind diese E2E-Tests der primäre Funktionsnachweis: Unit- und Integrationstests dürfen fehlende E2E-Szenarien nicht ersetzen.

## Schritt 3: Tests ausführen

Versuche zunächst den **Coverage-Befehl**. Schlägt dieser fehl (Tool nicht installiert, Plugin fehlt), führe stattdessen den normalen Test-Befehl aus und vermerke `Abdeckung: Nicht messbar`.

- Bei `dotnet test`: Verwende zusätzlich `--logger "console;verbosity=normal"` für strukturierte Ausgabe.
- Bei `pytest`: `--cov` erfordert `pytest-cov`; fehlt es, fällt der Befehl zurück auf `pytest -v`.
- Schlägt der Befehl mit einem Prozessfehler fehl (kein Kompilierfehler, sondern z. B. nicht installierte Abhängigkeiten), halte den Fehlertext im Ergebnis fest und setze Status `Ausführungsfehler`.

Prüfe anschließend, ob alle geplanten E2E-Szenarien im ausgeführten Testlauf enthalten waren und erfolgreich bestanden haben. Fehlt ein geplantes E2E-Szenario, wurde es nicht ausgeführt oder ist es fehlgeschlagen, gilt das Testergebnis als `Fehler vorhanden`.

## Schritt 4: Reale End-to-End-Abnahme

Zusätzlich zu den automatisierten Tests ist eine reale End-to-End-Abnahme durchzuführen, wenn die Anforderung mindestens einen der folgenden Bereiche berührt:

- Benutzeroberfläche (UI)
- Kommandozeilen-Schnittstelle (CLI)
- Externe Dienste oder Integrationen
- Persistenz (Datenbank, Dateisystem, Netzwerkzugriff)

Trifft keiner dieser Bereiche zu, entfällt die reale Abnahme — vermerke im Ergebnis `Reale Abnahme: Nicht erforderlich` mit kurzer Begründung.

Andernfalls: Führe die Anwendung bzw. den betroffenen Ablauf tatsächlich aus — real gestartet, nicht simuliert und nicht nur über das Test-Framework. Spiele die Akzeptanzszenarien aus `docs/features/{branchname}/plan.md` durch, mindestens aber den dort beschriebenen vollständigen Happy Path: vom realen externen oder Nutzer-Auslöser über alle Zwischenschritte bis zur sichtbaren Wirkung. Prüfe dabei die geplanten Ausgangszustände mit — insbesondere den Fall, dass erwartete Objekte, Konfigurationen oder Sitzungen noch nicht existieren. Enthalten die Szenarien Warte- oder Async-Abschnitte, prüfe zusätzlich die geplanten Wiederaufnahmewege.

**Zusatzanforderungen bei externen Werkzeug- und Protokollschnittstellen (z. B. MCP-Tools):**

- Zur realen Abnahme gehört mindestens ein echter Protokollaufruf (z. B. `tools/call`) gegen die laufende Anwendung mit Seed-/Demo-Daten — nicht nur ein Aufruf über das Test-Framework oder gegen Mocks.
- Verwende dabei reale Datensätze, die Domain-Randwerte ausüben (z. B. negative Beträge bei vorzeichenbehafteten Feldern, vollständige Enum-Werte, Null-Fälle). Genau diese Pfade werden von Mock-Annahmen in Unit-Tests verdeckt.

**Zusatzanforderungen bei UI-Szenarien:**

- Erstelle pro UI-Szenario Nachweis-Screenshots der sichtbaren Wirkung (mindestens ein Bild pro Szenario; bei mehreren relevanten Zuständen mehrere Bilder). Ohne Screenshots gilt die sichtbare Wirkung nicht als nachgewiesen.
- Prüfe bei jedem UI-Szenario zusätzlich zu den fachlichen Kriterien die Geometrie der Darstellung: Inhalt liegt vollständig innerhalb der Dialog-/Container-Grenzen (nichts ragt über Ränder hinaus), keine abgeschnittenen Texte oder Bedienelemente, und — sofern das Szenario eine relevante Größenabhängigkeit hat — das Verhalten auf einem schmalen Fenster bzw. Viewport.

Ergebnis je Szenario: `Bestanden`, `Fehlgeschlagen` oder `Nicht durchführbar`. `Nicht durchführbar` ist nur mit nachvollziehbarer Begründung zulässig (z. B. headless Umgebung ohne Desktop-Sitzung, fehlende externe Infrastruktur, fehlende Lizenzen) und gilt **nicht** als Erfolg.

`Nicht durchführbar` ist kein stiller Fehlschlag im Testlauf, sondern erfordert eine bewusste Anwender-Entscheidung — der Orchestrierer legt die betroffenen Szenarien samt Begründung vor und bietet zwei Auflösungswege:

- **Manuelle Abnahme durch den Anwender:** Der Anwender führt das Szenario selbst aus und bestätigt die sichtbare Wirkung. Das Ergebnis wird als `Bestanden (manuell, {Datum})` dokumentiert und zählt als erfolgreiche reale Abnahme.
- **Dokumentierter Verzicht:** Der Anwender verzichtet ausdrücklich mit Begründung. Das Ergebnis wird als `Verzichtet (Anwender, {Datum}, {Begründung})` dokumentiert.

Ohne eine dieser dokumentierten Entscheidungen bleibt das Szenario offen und wird wie ein fehlgeschlagener Test im Abschnitt `Fehlgeschlagene Tests` geführt.

### Sichtprüfung der Abnahme-Screenshots

Nach der Durchführung der UI-Szenarien ist **jeder** erzeugte Abnahme-Screenshot einzeln visuell zu prüfen — nicht auf Existenz oder Kriterienabdeckung, sondern auf Layout-Korrektheit. Öffne jedes Bild mit dem Datei-Lesewerkzeug der Umgebung und sichte es tatsächlich; eine Bewertung anhand von Dateinamen oder Konsolenausgaben ist keine Sichtprüfung.

Prüfkriterien pro Bild:

- Inhalt ragt über Container-, Dialog- oder Fensterränder hinaus (Überlauf, z. B. Tabellen oder Listen breiter als ihr Container)
- Abgeschnittene oder unleserliche Texte, Icons und Bedienelemente
- Fehlerhafte Breiten oder leere, ungenutzte Großflächen
- Überlagerungen, falsche Z-Ordnung oder sonstige Unleserlichkeit

Ergebnis pro Screenshot: `Unauffällig` oder `Defekt` mit kurzer Fehlerbeschreibung. Jeder `Defekt` wird zusätzlich im Abschnitt `Fehlgeschlagene Tests` geführt — die Sichtprüfung ist ein Prüfergebnis, keine Beobachtung.

Kann die Sichtprüfung technisch nicht durchgeführt werden (die Umgebung kann keine Bilder lesen), gilt sie wie `Nicht durchführbar` bei der realen Abnahme: kein Erfolg, sondern dokumentierte Anwender-Entscheidung — manuelle Sichtprüfung durch den Anwender (`Bestanden (manuell, {Datum})`) oder dokumentierter Verzicht (`Verzichtet (Anwender, {Datum}, {Begründung})`).

## Schritt 5: Fehlende Tests ermitteln

Werte die Coverage-Ausgabe aus. Falls keine Coverage-Daten vorhanden sind, suche stattdessen nach Quelldateien ohne korrespondierende Testdatei:

**Aus Coverage-Daten (bevorzugt):** Liste alle Quelldateien mit 0 % Zeilenabdeckung auf.

**Fallback ohne Coverage:** Für jede Quelldatei (z. B. `src/foo/bar.py`, `src/Foo/Bar.cs`, `internal/foo/bar.go`) prüfe, ob eine Testdatei mit passendem Namen existiert (z. B. `test_bar.py`, `BarTests.cs`, `bar_test.go`). Fehlt sie, gilt die Datei als ungetestet.

Ignoriere dabei generierte Dateien, Konfigurationsdateien und Einstiegspunkte (z. B. `Program.cs`, `main.go`, `__init__.py`).

## Schritt 6: Ergebnis ausgeben

Speichere das Ergebnis als `docs/features/{branchname}/test-results.md`:

```
# Test-Ergebnisse

## Ergebnis

**Status:** Keine Fehler | Fehler vorhanden | Ausführungsfehler | Kein Test-Runner gefunden

## Fehlgeschlagene Tests

Nur ausfüllen, wenn Status „Fehler vorhanden".

### {TestSuite / Dateiname}

- **{Testname}** — {Fehlermeldung in einer Zeile}

## E2E-Abdeckung

Nur ausfüllen, wenn geplante E2E-Szenarien vorhanden sind.

| Szenario | Test / Testklasse | Ergebnis |
|----------|-------------------|----------|
| ... | ... | Bestanden / Fehlend / Nicht ausgeführt / Fehlgeschlagen |

## Reale End-to-End-Abnahme

Nur ausfüllen, wenn eine reale Abnahme erforderlich war (UI, CLI, externe Dienste oder Persistenz betroffen). Andernfalls: `Reale Abnahme: Nicht erforderlich` mit Begründung.

| Szenario | Durchgeführt über | Ergebnis |
|----------|-------------------|----------|
| {Happy Path aus plan.md} | {z. B. gestartete Anwendung, CLI-Aufruf, manuelle UI-Bedienung} | Bestanden / Fehlgeschlagen / Nicht durchführbar / Bestanden (manuell) / Verzichtet |

## Sichtprüfung der Abnahme-Screenshots

Nur ausfüllen, wenn die reale Abnahme UI-Szenarien umfasste — andernfalls `Nicht erforderlich` mit kurzer Begründung.

**Status:** Alle geprüft, keine Defekte | Defekte vorhanden | Nicht durchführbar | Nicht erforderlich

Jeder erzeugte Screenshot wird einzeln aufgeführt:

| Screenshot | Szenario | Ergebnis |
|------------|----------|----------|
| {Datei} | {Szenario} | Unauffällig / Defekt: {Beschreibung} / Bestanden (manuell) / Verzichtet |

## Zusammenfassung

- Gesamt: {n}
- Bestanden: {n}
- Fehlgeschlagen: {n}
- Übersprungen: {n}

## Testabdeckung

**Abdeckung:** {n} % | Nicht messbar

| Datei | Abdeckung |
|-------|-----------|
| {Dateiname} | {n} % |

## Fehlende Tests

Nur ausfüllen, wenn Dateien mit 0 % Abdeckung oder ohne Testdatei gefunden wurden.
Quelle: `Coverage-Daten` | `Dateinamen-Konvention`

- `{Quelldatei}` — {Grund: „0 % Abdeckung" oder „Keine Testdatei gefunden"}
```

---

## Hinweise

- Nur fehlgeschlagene Tests auflisten — bestandene Tests nicht einzeln aufführen.
- Geplante E2E-Szenarien sind eine Ausnahme: Sie werden im Abschnitt `E2E-Abdeckung` vollständig aufgeführt, weil sie der wichtigste Funktionsnachweis für Benutzerflüsse sind.
- Fehlermeldungen auf eine Zeile kürzen; Stack-Traces weglassen.
- In der Abdeckungstabelle nur Dateien unter 80 % aufführen; Dateien mit 100 % weglassen.
- Das Ergebnis muss maschinell lesbar sein: Der Status in der ersten Zeile von „## Ergebnis" ist exakt einer von: `Keine Fehler`, `Fehler vorhanden`, `Ausführungsfehler`, `Kein Test-Runner gefunden`.
- Ein Check gilt nur mit einem expliziten Erfolgsergebnis als erfolgreich. `Ausführungsfehler`, `Kein Test-Runner gefunden`, abgebrochene oder übersprungene Läufe (`cancelled`, `skipped`), neutrale oder unbekannte Ergebnisse (`neutral`, `unknown`) sowie fehlende Artefakte gelten nicht als Erfolg.
- `Nicht durchführbar` bei der realen End-to-End-Abnahme gilt nicht als Erfolg und blockiert den Abschluss — es sei denn, der Anwender hat die Szenarien manuell abgenommen (`Bestanden (manuell)`) oder ausdrücklich dokumentiert verzichtet (`Verzichtet`). Die Entscheidung liegt immer beim Anwender, nicht beim ausführenden Agenten; der Agent darf ein `Nicht durchführbar` niemals selbst als erledigt markieren.
- Die Sichtprüfung der Abnahme-Screenshots ist bei UI-Szenarien Pflicht: `Status: Alle geprüft, keine Defekte` ist die einzige automatische Erfolgsform; `Defekte vorhanden` führt zu `Fehler vorhanden`, `Nicht durchführbar` erfordert dieselbe dokumentierte Anwender-Entscheidung wie die reale Abnahme. Fehlende Screenshots bei abgenommenen UI-Szenarien gelten als nicht durchgeführte Sichtprüfung.
