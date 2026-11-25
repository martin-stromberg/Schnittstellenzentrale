# Bestandsaufnahme zur Anforderung

Du bekommst eine übersetzte Anforderungsbeschreibung (typischerweise aus `requirement.md`). Analysiere den bestehenden Projektcode bezogen auf diese Anforderung und erstelle eine Bestandsaufnahme.

**Ziel:** Festhalten, was bereits existiert und welche Tests vor der Umsetzung der Anforderung funktionieren oder fehlschlagen. Keine Planung, keine Implementierungsvorschläge.

---

## Schritt 1: Anforderung einlesen

Lies die Datei `docs/features/{branchname}/requirement.md`. Entnimm daraus:
- Die betroffenen Klassen und Komponenten (Datenmodell, Logik, UI, Tests)
- Den fachlichen Bereich
- Relevante Eigenschaften, Events oder Mechanismen

## Schritt 2: Codeanalyse

Durchsuche `src/` gezielt nach den in der Anforderung genannten Klassen und verwandten Artefakten. Für jeden relevanten Bereich:

- **Datenmodellklassen:** Welche Eigenschaften existieren bereits?
- **Logikklassen:** Welche Methoden sind vorhanden? Welche Events werden abonniert oder publiziert?
- **Enums:** Welche Werte sind definiert?
- **Interfaces:** Welche Contracts sind definiert?
- **Tests:** Gibt es bestehende Testmethoden oder Hilfsmethoden für diesen Bereich?

Lies die relevanten Quelldateien vollständig, bevor du etwas dokumentierst.

## Schritt 3: Test-Ausgangszustand ermitteln

Führe vor jeder Änderung an Produktivcode, Tests oder Testkonfiguration die bestehenden Projekttests aus. Der Testlauf ist ein verpflichtender Bestandteil der Bestandsaufnahme und liefert den Nachweis für spätere Vergleiche.

1. Ermittle die projektüblichen Testbefehle und Voraussetzungen aus der Projektdokumentation, Build-/Testkonfiguration, vorhandenen Skripten und CI-Workflows. Berücksichtige alle Testsuiten des Projekts, nicht nur die von der Anforderung betroffenen Tests.
2. Halte vor dem Lauf den Ausgangszustand fest: Datum/Uhrzeit mit Zeitzone, Branch, Commit-ID, vorhandene uncommittete Änderungen sowie relevante Runtime-/SDK-Versionen und Testumgebung. Ein Commit allein beschreibt einen veränderten Arbeitsstand nicht vollständig; dokumentiere vorhandene Änderungen so, dass der getestete Stand nachvollziehbar bleibt.
3. Führe die ermittelten Testbefehle aus. Dokumentiere je Lauf den exakten Befehl einschließlich Filter, das Arbeitsverzeichnis, den Exit-Code sowie die Anzahl erfolgreicher, fehlgeschlagener und übersprungener Tests. Nicht ermittelte Zahlen als unbekannt kennzeichnen, nicht als null.
4. Sichere die Testausgaben und vorhandenen maschinenlesbaren Reports unter `docs/features/{branchname}/inventory/test-results/` und verlinke sie in `inventory/tests.md`. Entferne Zugangsdaten und andere Geheimnisse aus den gespeicherten Nachweisen.
5. Erfasse jeden fehlgeschlagenen Test mit vollständigem Testnamen bzw. Test-ID (bei parametrisierten Tests einschließlich Testfall), Suite/Dateipfad, Fehlermeldung und relevantem Stacktrace oder Report-Verweis. Wiederholungsläufe getrennt dokumentieren; ein später erfolgreicher Lauf darf einen vorherigen Fehlschlag nicht verdecken.
6. Dokumentiere nicht ausführbare, abgebrochene, deaktivierte oder übersprungene Tests samt konkretem Grund und betroffenem Umfang. Build-, Setup- und Infrastrukturfehler getrennt von tatsächlich fehlgeschlagenen Tests aufführen. Sind keine Tests vorhanden, halte dies ausdrücklich fest. Verändere oder repariere für die Bestandsaufnahme weder Code noch Tests oder Testkonfiguration.

**Nachweisregel für spätere Arbeiten:** Ein Fehler darf nur dann als bereits vorhanden ("preexisting") bezeichnet werden, wenn derselbe Testfall mit demselben Fehlerbild im dokumentierten Ausgangslauf nachweislich fehlgeschlagen ist. Ohne passenden Nachweis ist der Ausgangszustand unbekannt und der Fehler muss untersucht werden. Ein nicht ausgeführter oder übersprungener Test ist weder ein Erfolg noch ein Beleg für einen bereits vorhandenen Fehler. Den ursprünglichen Ausgangslauf unverändert erhalten; spätere Läufe separat ergänzen und Abweichungen bei Umgebung oder Testumfang offenlegen.

## Schritt 4: Dokumentation erstellen

Erstelle folgende Dateien:

### Hauptdokument: `docs/features/{branchname}/inventory.md`

Struktur:

```
# Bestandsaufnahme: {Feature-/Anforderungstitel}

Kurze Einleitung (1–2 Sätze): Welcher Bereich wurde analysiert, bezogen auf welche Anforderung?

## Zusammenfassung

Stichpunktliste der wesentlichen Befunde — was ist vorhanden, was fehlt offensichtlich noch.

Test-Ausgangszustand: Ergebnis der ausgeführten Tests, Anzahl bekannter Fehlschläge und verbleibende Testlücken mit Link auf den Nachweis in `inventory/tests.md`.

## Details

Für jeden analysierten Bereich ein Abschnitt mit Link zur Detaildatei:

- [Datenmodell](inventory/models.md)
- [Logik](inventory/logic.md)
- [Enums](inventory/enums.md)
- [Interfaces](inventory/interfaces.md)
- [Tests](inventory/tests.md)

Abschnitte, die leer wären (keine relevanten Funde), weglassen. Den Abschnitt Tests immer aufnehmen, auch wenn keine Tests vorhanden oder ausführbar sind.
```

### Detaildokumente: `docs/features/{branchname}/inventory/`

Für jeden relevanten Bereich eine eigene Datei. Nur anlegen, wenn es tatsächlich Inhalt gibt. `tests.md` immer als Nachweis des Test-Ausgangszustands anlegen.

**`models.md`** — Für jede betroffene Datenmodellklasse:
```
## `{Klassenname}`
Datei: `src/...`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| ...         | ... | ...                  |
```

**`logic.md`** — Für jede betroffene Logikklasse:
```
## `{Klassenname}`
Datei: `src/...`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| ...     | ...         | ...              |

Abonnierte Events: (falls vorhanden)
Publizierte Events: (falls vorhanden)
```

**`enums.md`** — Für jeden betroffenen Enum:
```
## `{Enumname}`
Datei: `src/...`

| Wert | Bedeutung |
|------|-----------|
| ...  | ...       |
```

**`interfaces.md`** — Für jedes betroffene Interface:
```
## `{Interfacename}`
Datei: `src/...`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| ...     | ...       | ...          | ...   |
```

**`tests.md`** — Übersicht bestehender Tests und Hilfsmethoden für den Bereich sowie projektweiter Test-Ausgangszustand:
```
## Test-Ausgangszustand vor der Umsetzung

- Zeitpunkt (mit Zeitzone): ...
- Branch und Commit-ID: ...
- Uncommittete Änderungen im getesteten Stand: ...
- Testumgebung und Runtime-/SDK-Versionen: ...
- Ermittelte Testsuiten und Quellen der Testbefehle: ...

### Testläufe

| Lauf | Befehl inkl. Filter | Arbeitsverzeichnis | Exit-Code | Erfolgreich | Fehlgeschlagen | Übersprungen | Nachweis |
|------|--------------------|--------------------|-----------|-------------|----------------|--------------|----------|
| ...  | ...                | ...                | ...       | ...         | ...            | ...          | [Log/Report](test-results/...) |

### Nachgewiesene bestehende Testfehler

| Test-ID inkl. Testfall | Suite / Dateipfad | Fehlerbild / Fehlermeldung | Lauf und Nachweis |
|-----------------------|------------------|---------------------------|-------------------|
| ...                   | ...              | ...                       | ...               |

Falls keine Testfehler nachgewiesen wurden, ausdrücklich angeben; bei unvollständigem Testlauf keine Aussage über nicht ausgeführte Tests treffen.

### Testlücken und Ausführungsprobleme

Nicht ausgeführte, übersprungene oder abgebrochene Tests/Suiten mit Grund und Umfang aufführen. Build-, Setup- und Infrastrukturfehler mit Nachweis dokumentieren. Falls keine Tests vorhanden sind, ausdrücklich vermerken.

## Testklassen

### `{Testklassenname}`
- `{Testmethode}` — Was wird getestet?

## Hilfsmethoden

### `{Hilfsklassenname}`
- `{Hilfsmethode}` — Welche Testdaten oder Abläufe werden bereitgestellt?
```

---

## Hinweise

- Nur dokumentieren, was tatsächlich im Code vorhanden ist. Keine Annahmen oder Vorausblicke.
- Klassen- und Methodennamen immer im Original und in Backticks.
- Querverweise zwischen Klassen explizit benennen (z. B. „wird aufgerufen von `OrderService`").
- Wenn eine Quelldatei für die Anforderung nicht relevant ist, nicht in die Dokumentation aufnehmen.
