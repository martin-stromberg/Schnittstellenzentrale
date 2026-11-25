# Technisches Code-Review

Führe ein technisches Code-Review der im aktuellen Branch geänderten Dateien durch.

**Ziel:** Qualitätsprobleme im neu geschriebenen Code aufdecken — keine funktionale Prüfung gegen den Plan (das ist Aufgabe von `/review-plan`), keine Änderungen am Code.

---

## Schritt 1: Geänderte Dateien ermitteln

Ermittle zunächst den Basisbranch des aktuellen Branches — `main` ist nur der letzte Fallback, nicht die Vorgabe:

1. **Explizit übergeben:** Wurde im aufrufenden Auftrag ein Basisbranch benannt (`Basisbranch: {name}`), verwende diesen.
2. **Namenskonvention:** Endet der aktuelle Branch-Name auf `-schritt-{n}-{slug}` (Ziffer gefolgt von Slug), ist der Basisbranch der Name ohne dieses Suffix.
3. **Reflog-Abzweig:** `git reflog show {branchname}` — der älteste Eintrag `checkout: moving from X to {branchname}` nennt den Branch, von dem abgezweigt wurde.
4. **Fallback `main`:** Nur verwenden, wenn 1–3 nichts liefern. Enthält die Diff-Liste dann auffällig viele Dateien, die nicht zur Anforderung gehören, dokumentiere das als Warnung im Ergebnis (möglicherweise falscher Basisbranch).

Führe anschließend folgenden Befehl aus, um alle im aktuellen Branch gegenüber dem ermittelten Basisbranch geänderten und neu erstellten Quelldateien zu ermitteln:

```
git diff --name-only --diff-filter=AM $(git merge-base HEAD {basisbranch})
```

Lies jede ermittelte Quelldatei vollständig.

## Schritt 2: Code-Review durchführen

Prüfe jede Datei anhand der folgenden Kriterien. Halte jeden Befund mit Dateireferenz und konkreter Beschreibung fest.

### Struktur und Verantwortlichkeiten

- **God-Klasse:** Hat eine Klasse mehr als eine klar abgrenzbare fachliche Verantwortlichkeit? Verletzt sie das Single-Responsibility-Prinzip?
- **God-Methode:** Ist eine einzelne Methode länger als ~50 Zeilen oder erledigt sie mehrere konzeptuell getrennte Aufgaben hintereinander?
- **Fehlende Kapselung:** Wird Logik, die wiederverwendbar wäre, inline wiederholt statt in eine eigene Methode ausgelagert?

### Doppelter Code

- Identische oder nahezu identische Codeblöcke innerhalb derselben Datei oder über mehrere neue Dateien hinweg
- Logik, die bereits in einer bestehenden Klasse vorhanden ist und hätte wiederverwendet werden können

### Namenskonventionen und Einheitlichkeit

- Sind Klassen-, Methoden- und Variablennamen im gesamten Branch einheitlich benannt? (z. B. durchgehend PascalCase für Typen, camelCase für Variablen)
- Weichen neue Namen vom Stil der bestehenden Codebasis ab?
- Methodennamen, die nicht beschreiben was die Methode tut (kryptische Abkürzungen, generische Namen wie `Process`, `Handle`, `DoStuff`)
- Inkonsistente Schreibweisen für dasselbe Konzept (z. B. `userId` vs. `user_id` vs. `UserId`)

### Kopplung und Erweiterbarkeit

- Direkte Abhängigkeiten zwischen Klassen, die besser über Interfaces oder Events entkoppelt werden sollten
- Fehlende Interfaces, wo Testbarkeit oder Austauschbarkeit erwartet wird
- Hardcodierte Werte, die in Konfiguration oder Konstanten gehören

### Fehlerbehandlung

- Fehlende Validierung von Eingaben oder Vorbedingungen vor kritischen Operationen
- **Validierung gegen den Domain-Vertrag:** Schränkt eine Validierung den Wertebereich stärker ein als das Domain-Modell (Vorzeichen, Enum-Vollständigkeit, Null-Semantik)? Referenz ist der Domain-Code, nicht die Annahme des Implementierers. Gilt besonders für externe Schnittstellen (Werkzeuge, APIs): sie dürfen Syntax und Pflichtfelder prüfen, semantische Wertebereiche gehören an die Domain bzw. den Service — nie strenger als der Domain-Vertrag. Ebenso Befund: Fehlt eine von der Domain geforderte Validierung?
- Zu breite Exception-Handler (`catch (Exception)`) ohne sinnvolle Behandlung
- Exceptions, die still geschluckt werden (leere catch-Blöcke)
- Fehlermeldungen ohne aussagekräftigen Kontext

### Testqualität

- Testmethoden, die mehr als einen fachlichen Fall prüfen (fehlende Trennung)
- Fehlende Arrange-Act-Assert-Struktur
- Tests, die interne Implementierungsdetails statt fachliches Verhalten prüfen
- Fehlende oder unzureichende Testabdeckung für neue öffentliche Methoden

### Randfälle bei datenabhängigen Prüfungen

- **Grenzwert bei Kollisions- und Abdeckungsprüfungen:** Stammt eine Grenze (Endzeitpunkt, letzter Stand, Abdeckungsgrenze) aus den Daten selbst, prüfe die Vergleichsoperatoren auf den Gleichheitsfall (`>` vs. `>=`, `<` vs. `<=`). Befund, wenn der Rand falsch einbezogen oder ausgeschlossen wird, sodass ein Kollisions- oder Abdeckungsschutz genau an dieser Grenze nicht greift.
- **Rückblickfenster:** Berechnet eine Prüfung ein Fenster zurück ab einer datenabhängigen Grenze, prüfe, ob der Fensterbeginn den Randwert korrekt einschließt. Befund, wenn ein Filter „ab hier relevant“ mit datenabhängiger Grenze ohne Prüfung des Randes arbeitet.

### Ablaufverantwortung bei mehrstufigen Vorgängen

- **Mehrere öffentliche Einzelschritte:** Bietet ein Service mehrere öffentlich aufrufbare Einzelschritte eines mehrstufigen Geschäftsvorgangs (Verarbeitung, Aktualisierung, Pflege) an, deren Reihenfolge oder Vorbedingungen nur im Aufrufer stehen? Befund mit Empfehlung, einen Einstiegspunkt pro Geschäftsvorgang im Service zu kapseln.
- **Zweiter Ausführungspfad:** Bildet ein Worker, eine Wartungsfunktion oder ein anderer Service die Schrittfolge eines Vorgangs selbst nach, statt den Einstiegspunkt des zuständigen Services aufzurufen? Befund mit Empfehlung, den Einstiegspunkt zu verwenden.
- Die Kategorie gilt für alle mehrstufigen Vorgänge eines Services, unabhängig davon, ob ein Hintergrund-Worker beteiligt ist.

### Charakterisierungs- und Ablauftests

- **Künstlicher Ablauf:** Setzt ein Test einen mehrstufigen Vorgang aus Einzelschritten in einer Reihenfolge zusammen, die produktiv nicht vorkommt, statt den produktiven Einstiegspunkt zu verwenden? Befund mit Empfehlung, den Einstiegspunkt im Test zu verwenden.
- **Eingefrorener Fehler:** Schreibt ein Test einen Zustand oder ein Ergebnis als erwartet fest, das nur aus einer Abweichung vom produktiven Ablauf oder aus einem bekannten Fehler entsteht (z. B. grün, weil ein Schritt fehlt)? Befund mit Empfehlung, die Erwartung am fachlich korrekten Verhalten auszurichten.

### Klassische Code Smells

**Datenstrukturen**
- **Primitive Obsession:** Primitive Typen (`string`, `int`, `bool`) werden statt kleiner spezialisierter Klassen/Value-Objects verwendet (z. B. `string` für E-Mail, Währung, Status)
- **Long Parameter List:** Methoden mit mehr als 3–4 Parametern, die besser als Parameter-Objekt oder Builder modelliert würden
- **Data Clumps:** Datengruppen (z. B. `street`, `city`, `zip`), die stets gemeinsam auftreten, aber nicht als eigenes Objekt zusammengefasst sind

**Verantwortlichkeit und Kopplung**
- **Feature Envy:** Eine Methode greift mehr auf Daten/Methoden einer fremden Klasse zu als auf die eigene — Indikator, dass die Logik in die falsche Klasse gehört
- **Inappropriate Intimacy:** Eine Klasse kennt und nutzt private Felder oder interne Implementierungsdetails einer anderen Klasse direkt
- **Message Chains:** Methodenketten über mehrere Ebenen (`a.GetB().GetC().GetD()`) — Verletzung des Law of Demeter, erhöht Kopplung und Fragility
- **Middle Man:** Eine Klasse delegiert nahezu alle Aufrufe an eine andere, ohne eigene Logik hinzuzufügen

**Überflüssiger Code**
- **Lazy Class:** Eine Klasse enthält so wenig Logik, dass ihre Existenz als eigene Klasse nicht gerechtfertigt ist
- **Speculative Generality:** Abstraktion, Interfaces oder Parameter für hypothetische zukünftige Anforderungen, die aktuell nicht benötigt werden
- **Switch Statements / Type Checks:** `switch`- oder `if/else`-Ketten über Typprüfungen (`is`, `typeof`, Enum-Flags), die durch Polymorphismus oder Strategy-Pattern ersetzt werden könnten
- **Temporäres Feld:** Instanzfelder, die nur in bestimmten Situationen gesetzt sind und sonst `null` bleiben — deutet auf eine fehlende Klasse oder Methode hin

### Toter Code

- Methoden, die angelegt, aber nirgends aufgerufen werden
- Auskommentierter Code
- Variablen oder Parameter, die deklariert, aber nicht verwendet werden

## Schritt 3: Review-Ergebnis ausgeben

Speichere das Ergebnis als `docs/features/{branchname}/review-code.md` (überschreibe eine vorhandene Datei):

```
# Code-Review

## Ergebnis

**Status:** Keine Befunde | Befunde vorhanden

## Befunde

Nur ausfüllen, wenn Status „Befunde vorhanden". Für jeden Befund:

### {Dateiname} ({Klassenname})

- **{Kategorie}** — {Konkrete Beschreibung des Problems, Zeilennummer oder Methodenname wenn möglich}

  Empfehlung: {Was genau geändert werden soll}

## Geprüfte Dateien

Liste aller geprüften Dateien:
- `src/...`
```

---

## Hinweise

- Nur die im aktuellen Branch geänderten und neu erstellten Dateien reviewen.
- Jeden Befund so konkret formulieren, dass der Implementierungsagent ohne Rückfrage handeln kann.
- Keine subjektiven Stilpräferenzen als Befunde aufnehmen — nur Verstöße gegen die oben genannten Kriterien.
- Das Review-Ergebnis muss maschinell lesbar sein: Der Status in der ersten Zeile von „## Ergebnis" ist entweder exakt `Keine Befunde` oder `Befunde vorhanden`.
