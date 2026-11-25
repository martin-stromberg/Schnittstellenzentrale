# Umsetzungsplanung

Du bekommst eine übersetzte Anforderung (`requirement.md`) und eine Bestandsaufnahme (`inventory.md` inkl. Detaildokumente). Erstelle daraus einen konkreten Umsetzungsplan für die Implementierung.

**Ziel:** Einen vollständigen, umsetzbaren Plan erstellen — keine Implementierung, keine Codeänderungen.

---

## Schritt 1: Eingaben einlesen

Lies folgende Dateien vollständig:

- `docs/features/{branchname}/requirement.md` — übersetzte Anforderung
- `docs/features/{branchname}/inventory.md` — Übersicht der Bestandsaufnahme
- Alle verlinkten Detaildokumente unter `docs/features/{branchname}/inventory/`

Wurden zusammen mit den Eingaben **Antworten auf offene Punkte** übergeben, notiere sie als geklärt. Diese Antworten sind in den Plan einzuarbeiten (z. B. als Designentscheidungen oder Programmablaufdetails) und dürfen in der Tabelle der offenen Punkte nicht mehr erscheinen.

## Schritt 2: Lückenanalyse

Stelle für jeden in der Anforderung genannten Bereich fest:

- Was ist bereits vorhanden (aus Bestandsaufnahme)?
- Was fehlt noch (neue Klassen, neue Felder, neue Methoden, neue Events)?
- Was muss angepasst werden (bestehende Klassen erweitern)?
- Welche Programmabläufe (Sequenzen von Methodenaufrufen, Events, Datenbankzugriffen) sind neu oder ändern sich?
- Welche Datenbankmigrationen sind notwendig?
- Welche Validierungsregeln sind für neue oder geänderte Eingaben erforderlich?
- Bei neuen oder geänderten externen Schnittstellen (Werkzeuge, APIs, z. B. MCP-Tools): Welche Feldkonventionen gelten pro Feld — Vorzeichen, Einheit, Null-Semantik? Quelle ist der normative Toolkatalog des Projekts bzw. das Domain-Modell, nicht die Annahme des Implementierers.
- Welche Konfigurationseinträge werden hinzugefügt oder geändert?
- Welche bestehenden Features oder Klassen sind von den Änderungen betroffen (Seiteneffekte)?
- Welche bestehenden Tests brechen durch die Änderungen?
- Welche neuen E2E-Tests sind für die geänderten oder neuen Benutzerabläufe erforderlich?
- Welche bestehenden E2E-Tests sind betroffen?
- Welche E2E-Tests sind als primärer Funktionsnachweis nötig, weil Unit- oder Integrationstests den tatsächlichen Benutzerfluss nicht ausreichend absichern?
- Welche Designentscheidungen stehen für neue Komponenten an, bei denen mehrere sinnvolle Ansätze existieren?
- Welche Voraussetzungen fehlen im Repo für die geplanten Schritte? (fehlende NuGet-Pakete, Basisklassen, Testinfrastruktur, Konfigurationsklassen — alles, ohne das ein Schritt nicht ausführbar wäre)
- Welche fachlichen Objekte, Konfigurationen oder Sitzungen setzt der Ablauf voraus, könnten aber zur Laufzeit noch nicht existieren? Wie ist mit jedem umzugehen (erzeugen, wiederverwenden, aktivieren, verwerfen)?
- Welche Wartesituationen, asynchronen Prozesse oder externen Abhängigkeiten gibt es? Für jede: Welche Zustände und Übergänge entstehen, und über welchen wirksamen Weg wird der Ablauf nach einer erwartbaren Unterbrechung wieder aufgenommen?
- Welche sichtbaren Benutzeraktionen könnten den beschriebenen nächsten Schritt nicht tatsächlich auslösen — und wie wird die Blockade dem Anwender in diesen Fällen eindeutig erklärt?
- Bei UI-Schritten: Welche bestehenden, vergleichbaren UI-Muster (Such-, Auswahl-, Lookup-Komponenten o. ä.) aus der Bestandsaufnahme sind für die geforderte Interaktion wiederverwendbar? Enthält die Anforderung Auswahl-/Identifikationsaufgaben über benannte Entitäten?

## Schritt 3: Plan erstellen

Erstelle die Datei `docs/features/{branchname}/plan.md` mit folgender Struktur:

```
# Umsetzungsplan: {Feature-/Anforderungstitel}

## Übersicht

Kurze Beschreibung (2–3 Sätze): Was wird umgesetzt, welche Bereiche sind betroffen?

## Scope-Klärung

Verbindliche fachliche Abgrenzung vor der Umsetzung — übernommen bzw. konkretisiert aus dem Abschnitt `Fachlicher Scope` der `requirement.md`:

|| Feld | Festlegung |
||------|------------|
|| Verarbeitungseinheit | {fachliches Objekt/Ereignisbündel} |
|| Auslösende Ereignisse | {konkrete Auslöser} |
|| Erwartete Wirkung | {sichtbares Ergebnis} |
|| Umgang mit fehlenden Objekten | {je Objekt: erzeugen / wiederverwenden / aktivieren / verwerfen} |

### Scope-Ausschlüsse

Jede Einschränkung gegenüber einer naheliegenden Nutzererwartung ist hier als bewusster Ausschluss mit Begründung und Status zu führen. Der Status `offen` wechselt erst auf `bestätigt`, wenn der Anwender den Ausschluss in Schritt 5a bestätigt hat (mit Datum vermerken).

|| # | Einschränkung / Ausschluss | Begründung | Status |
||---|----------------------------|------------|--------|
|| 1 | ... | ... | offen / bestätigt |

Falls keine Einschränkungen bestehen: „Keine."

## Designentscheidungen

Für jede neue Komponente oder jeden Ablauf, bei dem mehrere sinnvolle Strukturansätze existieren, wird hier festgehalten, welcher Ansatz gewählt wird und warum.

| Komponente / Bereich | Gewählter Ansatz | Begründung |
|----------------------|-----------------|------------|
| `...` | ... | ... |

Falls alle Designentscheidungen durch bestehende Konventionen eindeutig vorgegeben sind: „Keine — folgt bestehenden Mustern."

## Programmabläufe

Beschreibung der Abläufe, die implementiert werden sollen. Für jeden Ablauf ein Unterabschnitt:

### {Ablaufname}

Schritt-für-Schritt-Beschreibung des Ablaufs (kein Code — Methodennamen, Klassen und Events in Backticks):

1. ...
2. ...

Beteiligte Klassen/Komponenten: `...`, `...`

## Neue Klassen

Liste aller neu zu erstellenden Klassen:

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `...`  | Klasse / Datenmodellklasse / Enum / Interface / ... | ... |

## Änderungen an bestehenden Klassen

Für jede zu ändernde Klasse ein Abschnitt:

### `{Klassenname}` ({Typ})

- **Neue Eigenschaften:** `{Name}` ({Typ}) — Zweck
- **Neue Methoden:** `{Methodenname}` — Zweck, Parameter, Rückgabewert
- **Geänderte Methoden:** `{Methodenname}` — Was ändert sich und warum?
- **Neue Events:** `{Eventname}` — Wann wird es ausgelöst?
- **Neue Event-Handler:** Auf welches Event wird reagiert?

## Datenbankmigrationen

Welche Migrationen sind erforderlich?

| Migrationsname | Betroffene Tabellen/Spalten | Beschreibung der Änderung |
|----------------|----------------------------|---------------------------|
| `...`          | ...                        | ...                       |

Falls keine Migrationen erforderlich sind: „Keine."

## Validierungsregeln

Welche Validierungen müssen für neue oder geänderte Eingaben implementiert werden?

Externe Schnittstellen (Werkzeuge, APIs) validieren nur Syntax und Pflichtfelder; semantische Wertebereichsprüfungen (Vorzeichen, Enum-Vollständigkeit, Null-Semantik) bleiben in der Domain bzw. im Service. Der Plan darf an der Schnittstelle keine Regel vorsehen, die den Wertebereich enger zieht als der Domain-Vertrag.

| Feld / Objekt | Regel | Fehlerfall |
|---------------|-------|------------|
| `...`         | ...   | ...        |

Falls keine Validierungen erforderlich sind: „Keine."

## Konfigurationsänderungen

Welche Einträge in Konfigurationsklassen oder `appsettings` werden hinzugefügt oder geändert?

| Eintrag | Typ | Standardwert | Zweck |
|---------|-----|--------------|-------|
| `...`   | ... | ...          | ...   |

Falls keine Konfigurationsänderungen erforderlich sind: „Keine."

## Seiteneffekte und Risiken

Welche bestehenden Features, Klassen oder Abläufe werden durch die Änderungen beeinflusst?

- **{Bereich}:** {Beschreibung des Seiteneffekts oder Risikos}

Falls keine bekannten Seiteneffekte bestehen: „Keine bekannten Seiteneffekte."

## Umsetzungsreihenfolge

Geordnete Liste der Arbeitsschritte. Jeder Schritt listet explizit, was vor seiner Ausführung im Repo vorhanden sein muss. Fehlt eine Voraussetzung noch, muss sie als eigener früherer Schritt erscheinen.

1. **{Aufgabe}**
   - Voraussetzungen: {NuGet-Pakete, Basisklassen, Konfigurationseinträge, Interfaces — die im Repo bereits vorhanden oder durch frühere Schritte angelegt sein müssen. „Keine" wenn der Schritt voraussetzungslos ist.}
   - Beschreibung: {Was in diesem Schritt konkret getan wird}

2. ...

## Tests

### Neue Tests

Welche neuen Testmethoden und Hilfsmethoden sind erforderlich?

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| ... | ... | ... |

### Betroffene bestehende Tests

Welche vorhandenen Tests müssen angepasst werden, weil sich Signaturen, Verhalten oder Datenstrukturen ändern?

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| ... | ... |

Falls keine bestehenden Tests betroffen sind: „Keine."

### E2E-Tests (primärer Funktionsnachweis)

Für jede neue oder geänderte Benutzerinteraktion muss E2E-Abdeckung geplant werden. E2E-Tests sind bei Benutzerflüssen der wichtigste Nachweis, dass die Anwendung tatsächlich funktioniert; Unit- und Integrationstests ergänzen diese Abdeckung, ersetzen sie aber nicht.

Der Happy Path jedes neuen oder geänderten Benutzerflusses muss durch einen E2E-Test abgedeckt sein. Fehlerfälle, Berechtigungs-/Sichtbarkeitsregeln und kritische Edge Cases müssen ebenfalls als E2E-Szenarien geplant werden, sofern sie für Anwender sichtbar oder über die UI auslösbar sind.

Bei Dialogen, Overlays und Panels sind **geometrische Assertions** einzuplanen: Der E2E-Test prüft per BoundingBox-Vergleich, dass der Inhalt vollständig innerhalb der Grenzen des Dialogs bzw. Containers liegt — Layout-Überlauf wird damit CI-erzwungen statt aufmerksamkeitsabhängig (Referenzimplementierung: `PostingRulesOverlay_OpensAndCloses_ViaRibbon`, FinanceManager Commit `0a94bb74`).

| Priorität | Szenario | Testdatei / Testklasse | Abgedecktes Akzeptanzkriterium | Warum E2E nötig ist |
|-----------|----------|------------------------|-------------------------------|-------------------|
| Pflicht | ... | ... | ... | ... |

Welche bestehenden E2E-Tests müssen angepasst werden?

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| ... | ... |

Falls keine bestehenden E2E-Tests betroffen sind: „Keine."

Falls eine Anforderung keinen Benutzerfluss berührt, begründe kurz, warum keine E2E-Tests erforderlich sind. Ein bloßer Verweis auf vorhandene Unit- oder Integrationstests reicht als Begründung nicht aus.

## Akzeptanzszenarien

Überprüfbare Szenarien, die den vollständigen Weg vom Auslöser bis zur sichtbaren Wirkung beschreiben — nicht auf Komponentenebene, sondern als vollständiger Nutzer- bzw. Systemablauf.

Mindestens ein Szenario muss den vollständigen Happy Path abdecken: vom externen oder Nutzer-Auslöser über alle Zwischenschritte bis zur sichtbaren Wirkung.

Jedes Szenario benennt den relevanten Ausgangszustand — insbesondere den Fall, dass erwartete interne Objekte, Konfigurationen oder Sitzungen noch nicht existieren (gemäß der Festlegung in `Scope-Klärung` → „Umgang mit fehlenden Objekten").

Bei Wartesituationen, asynchronen Prozessen oder externen Abhängigkeiten ist je Szenario festzulegen: Zustand während des Wartens, auslösender Übergang und der wirksame Wiederaufnahmeweg (wer/was setzt den Ablauf fort, und woher weiß der Nutzer das?).

|| # | Szenario | Auslöser | Ausgangszustand | Schritte | Sichtbare Wirkung | Nachweis |
||---|----------|----------|------------------|----------|-------------------|----------|
|| 1 | {Happy Path} | {externer/Nutzer-Auslöser} | {inkl. fehlender Objekte} | {Ablauf} | {sichtbares Ergebnis} | {Test / reale Abnahme} |
|| 2 | {Fehlende Voraussetzung / Warte- / Fehlerfall} | ... | ... | ... | ... | ... |

## Offene Punkte

Ungeklärte technische oder fachliche Fragen, die vor oder während der Implementierung geklärt werden müssen.

**Wichtig:** Punkte, auf die in diesem Durchlauf Antworten vorlagen, erscheinen hier nicht mehr — sie gelten als geklärt und sind bereits im Plan eingearbeitet.

Falls es offene Punkte gibt, wird für jeden — sofern möglich — ein konkreter Lösungsvorschlag angegeben:

| # | Offener Punkt | Empfohlener Vorschlag |
|---|---------------|----------------------|
| 1 | ... | ... |

Falls keine offenen Punkte bestehen: „Keine."
```

## Schritt 4: Tasks-Datei erstellen

Erstelle die Datei `docs/features/{branchname}-tasks.md` — **nicht** im Branch-Unterverzeichnis, da sie nach dem Aufräumen erhalten bleiben soll.

Die Datei enthält alle Einzelaufgaben aus dem Plan in einer thematisch geordneten Tabelle. Themen sind funktionale Bereiche (z. B. „Datenmodell", „Logik", „UI", „Validierung", „Konfiguration", „Tests", „E2E-Tests") — keine Implementierungsreihenfolge.

Jede Zeile entspricht einer abgrenzbaren Einzelaufgabe: eine Klasse anlegen, eine Methode hinzufügen, eine Migration erstellen, einen Test schreiben. Keine Sammelzeilen wie „alle neuen Klassen anlegen".

```
# Tasks: {Feature-/Anforderungstitel}

| # | Bereich | Aufgabe | Status | Testnachweis |
|---|---------|---------|--------|--------------|
| 1 | {Bereich} | {Konkrete Einzelaufgabe, z. B. "`OrderStatus` Enum anlegen"} | Offen | — |
| 2 | ... | ... | Offen | — |
```

**Befüllung:** Alle Zeilen werden mit Status `Offen` und Testnachweis `—` initialisiert. Aktualisierung erfolgt durch `/review-plan`.

---

## Hinweise

- Nur planen, was die Anforderung verlangt — keine Extras, keine spekulativen Erweiterungen.
- Technische Bezeichnungen (Klassen- und Methodennamen) immer im Original und in Backticks.
- Abhängigkeiten zwischen Klassen explizit benennen (z. B. „Enum muss vor der Datenmodellklasse angelegt werden").
- Jeder Schritt in der Umsetzungsreihenfolge muss ausführbar sein: Alle Voraussetzungen (NuGet-Pakete, Basisklassen, Testinfrastruktur) müssen entweder im Repo bereits vorhanden sein oder in einem früheren Schritt explizit angelegt werden. Kein Schritt darf stillschweigend auf etwas Fehlendes aufbauen.
- Designentscheidungen nur dort explizit ausformulieren, wo echte Alternativen bestehen. Erweiterungen bestehender Klassen folgen den vorhandenen Mustern — kein Eintrag nötig.
- Als Vokabular für Designentscheidungen PoEAA-Muster verwenden, wenn sie passen: Repository, Value Object, Service Layer, Data Mapper, Transaction Script, Domain Model, Gateway, Specification u. a.
- Wenn Designentscheidungen mehrere Optionen haben, die eine bevorzugte Option benennen und kurz begründen — keine Abwägungslisten.
- Keine Implementierungsdetails (kein Code) — nur Struktur und Absicht.
- Für Benutzerflüsse sind E2E-Tests als primärer Funktionsnachweis zu planen. Fehlende E2E-Abdeckung darf nur begründet werden, wenn die Anforderung keinen über UI oder Nutzeraktion erreichbaren Ablauf betrifft.
- Eine sichtbare Benutzeraktion darf nur geplant werden, wenn sie den beschriebenen nächsten Schritt tatsächlich auslösen kann. Kann sie es (z. B. wegen fehlender Voraussetzungen) nicht, muss der Plan festlegen, wie die Blockade dem Anwender eindeutig erklärt wird (deaktiviertes Element plus verständliche Begründung/Hinweis) — eine wirkungslose, scheinbar aktive Aktion ist kein zulässiger Planansatz.
- Der Abschnitt `Scope-Klärung` ist Pflicht: Jedes erwartete fachliche Objekt, das zur Laufzeit fehlen kann, braucht eine Festlegung (erzeugen / wiederverwenden / aktivieren / verwerfen). Scope-Ausschlüsse werden mit Status `offen` angelegt und erst nach Anwenderbestätigung auf `bestätigt` gesetzt.
- Der Abschnitt `Akzeptanzszenarien` ist Pflicht und enthält mindestens einen vollständigen Happy Path vom externen oder Nutzer-Auslöser bis zur sichtbaren Wirkung sowie die relevanten Ausgangszustände (inkl. noch nicht vorhandener Objekte, Konfigurationen oder Sitzungen).
- Ausgabe-Design an externen Schnittstellen: Aggregat- und Übersichtsfelder kennzeichnen geerbte oder übergeordnete Werte explizit (z. B. ein Feld `parentBudget` neben `budgetSum`), damit `0`/`null` nicht als „nicht konfiguriert" fehlinterpretiert wird.
- Bei UI-Schritten muss der Plan explizit auf bestehende, vergleichbare UI-Muster im Projekt verweisen (aus der Bestandsaufnahme) und deren Wiederverwendung vorschreiben — keine Implementierungsfreiheit für „irgendeine Eingabemöglichkeit". Verlangt die Anforderung eine Auswahl benannter Entitäten, muss der Plan eine Suche, ein Browsing oder ein Klartext-Dropdown festschreiben; die Eingabe interner Kennungen (Datenbank-Ids, GUIDs, interne Codes) durch Endanwender ist kein zulässiger Planansatz.
- Für jeden offenen Punkt einen Empfehlungsvorschlag angeben, wenn die Antwort aus Anforderung, Bestandsaufnahme oder gängiger Praxis ableitbar ist. Falls ein Punkt wirklich ungeklärt ist und kein sinnvoller Vorschlag möglich ist, „Kein Vorschlag möglich — Klärung erforderlich" eintragen.
- Wurden Antworten auf offene Punkte übergeben, müssen alle geklärten Punkte vollständig aus der Tabelle entfernt werden. Ein beantworteter Punkt darf in der aktualisierten `plan.md` nicht mehr erscheinen — auch nicht mit dem Status „geklärt".
