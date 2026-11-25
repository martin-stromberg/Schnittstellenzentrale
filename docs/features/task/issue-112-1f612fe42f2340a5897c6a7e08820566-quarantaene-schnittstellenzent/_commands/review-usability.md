# Usability-Review aus Endanwendersicht

Führe ein Usability-Review der im aktuellen Branch geänderten Oberflächen durch.

**Ziel:** Prüfen, ob eine normale, nicht-technische Person die in der Anforderung beschriebenen Aufgaben mit der gebauten Oberfläche tatsächlich erledigen kann — nicht, ob die Funktion irgendwo im Code vorhanden ist. Keine Code-Qualitätsprüfung (das ist Aufgabe von `/review-code`), keine Plantreue-Prüfung (das ist Aufgabe von `/review-plan`), keine Änderungen am Code.

**Wichtig:** Dieses Review wird bewusst ohne Plan- und Review-Kontext durchgeführt. Lies weder `plan.md` noch `review.md` oder `review-code.md` — die Prüfung soll ausschließlich aus der ursprünglichen Kundenanforderung und der tatsächlich gebauten Oberfläche erfolgen, um Betriebsblindheit zu vermeiden.

---

## Schritt 1: Anforderung einlesen

Lies `docs/features/{branchname}/requirement.md` vollständig. Liste jede darin beschriebene Benutzerinteraktion (Aktionen wie „auswählen", „hinzufügen", „ändern", „zuordnen" usw.).

## Schritt 2: Geänderte UI-Dateien ermitteln

Ermittle zunächst den Basisbranch des aktuellen Branches — `main` ist nur der letzte Fallback, nicht die Vorgabe:

1. **Explizit übergeben:** Wurde im aufrufenden Auftrag ein Basisbranch benannt (`Basisbranch: {name}`), verwende diesen.
2. **Namenskonvention:** Endet der aktuelle Branch-Name auf `-schritt-{n}-{slug}` (Ziffer gefolgt von Slug), ist der Basisbranch der Name ohne dieses Suffix.
3. **Reflog-Abzweig:** `git reflog show {branchname}` — der älteste Eintrag `checkout: moving from X to {branchname}` nennt den Branch, von dem abgezweigt wurde.
4. **Fallback `main`:** Nur verwenden, wenn 1–3 nichts liefern. Prüfe danach die Plausibilität: Enthält die Diff-Liste auffällig viele Dateien, die nicht zu dieser Anforderung gehören, dokumentiere das als Warnung im Ergebnis (zu breiter Diff — möglicherweise falscher Basisbranch) statt stillschweigend auf der zu breiten Liste zu prüfen.

Führe anschließend folgenden Befehl aus, um alle im aktuellen Branch gegenüber dem ermittelten Basisbranch geänderten und neu erstellten Dateien zu ermitteln:

```
git diff --name-only --diff-filter=AM $(git merge-base HEAD {basisbranch})
```

Filtere die Liste auf UI-relevante Dateien (z. B. `*.razor`, `*.cshtml`, `*.xaml`, Formular-/Dialog-Codebehind, zugehörige ViewModels soweit sie Bedienelemente und Eingabefelder definieren). Lies jede dieser Dateien vollständig.

Enthält die Änderungsliste **keine** UI-relevanten Dateien oder beschreibt die Anforderung keine Benutzerinteraktion, ist das Review nicht anwendbar: Schreibe `review-usability.md` mit Status `Keine Befunde` und dem Hinweis `Keine UI-Änderungen bzw. keine Benutzerinteraktion in der Anforderung — Review nicht anwendbar.` und beende das Kommando.

## Schritt 3: Usability-Prüfung durchführen

Versetze dich in eine nicht-technische Endanwenderin ohne Kenntnis der Datenmodell-Struktur. Prüfe für jede in Schritt 1 identifizierte Interaktion anhand der gebauten Oberfläche:

### Interne/technische Identifikatoren

- Muss der Anwender an irgendeiner Stelle einen internen oder technischen Wert eingeben oder kennen — Datenbank-Id, interner Code, GUID, Dateiname, technischer Schlüssel — den er im normalen Gebrauch der Anwendung nicht kennen kann?
- Typisches Fehlbild: ein Zahlen- oder Textfeld, in das eine „Medien-Id", „Benutzer-Id" o. ä. eingetippt werden muss, statt den Datensatz über Name/Suche auszuwählen.

### Auswahl- und Identifikationsaufgaben

- Fordert die Anforderung eine Auswahl aus einer Menge benannter Entitäten („wähle einen Film/eine Serie/einen Titel/einen Benutzer aus")?
- Gibt es dafür eine Suche, Autovervollständigung, ein Dropdown mit Klartext oder eine Browsing-Möglichkeit — oder wird stillschweigend eine bereits bekannte Kennung vorausgesetzt?
- Bei potenziell großen oder wachsenden Mengen: Reicht eine reine Liste/Dropdown überhaupt aus, oder fehlt eine Suchmöglichkeit?

### Wiederverwendung etablierter Muster

- Existieren im Projekt bereits Such-, Auswahl- oder Lookup-Komponenten für vergleichbare Aufgaben (z. B. eine bestehende Medien-Suche oder Benutzerauswahl)? Prüfe dies anhand der Projektstruktur.
- Wurde stattdessen eine abweichende, primitivere Lösung neu erfunden (z. B. rohes Id-Feld statt der vorhandenen Suchkomponente)?

### Erreichbarkeit und Beschriftung

- Sind alle in der Anforderung genannten Handlungen über sichtbare, verständlich beschriftete Bedienelemente erreichbar — ohne Vorwissen über die Datenmodell-Struktur?
- Verlangt eine Eingabe implizites Wissen (z. B. „Welche Id hat Film X?"), das ein Anwender nicht haben kann?

### Aktionswirksamkeit

- Löst jede sichtbare, neu oder geändert angebotene Benutzeraktion (Button, Menüeintrag, Link) den in der Anforderung beschriebenen nächsten Schritt tatsächlich aus — ist ein Handler verdrahtet, führt der Klick zum erwarteten Ergebnis?
- Gibt es Aktionen, die sichtbar bzw. aktiv wirken, aber nichts bewirken oder in eine Sackgasse führen (kein Handler, kein Folgeschritt, Endlosspinner ohne Wiederaufnahme)?
- Kann eine Aktion in bestimmten Zuständen nicht wirken (fehlende Voraussetzungen, fehlende Objekte, laufender Async-Prozess)? Ist die Blockade dann für den Anwender eindeutig erklärt — deaktiviertes Element plus verständlicher Grund oder Hinweis — statt einer scheinbar aktiven, aber wirkungslosen Aktion?

## Schritt 4: Review-Ergebnis ausgeben

Speichere das Ergebnis als `docs/features/{branchname}/review-usability.md` (überschreibe eine vorhandene Datei):

```
# Usability-Review

## Ergebnis

**Status:** Keine Befunde | Befunde vorhanden

## Befunde

Nur ausfüllen, wenn Status „Befunde vorhanden". Für jeden Befund:

### {Dateiname} ({Komponente/Seite})

- **{Kategorie: Interne Kennung | Fehlende Suche/Auswahl | Abweichendes Muster | Erreichbarkeit | Wirkungslose Aktion | Unerklärte Blockade}** — {Konkrete Beschreibung: Welche Interaktion aus der Anforderung ist betroffen, was müsste ein Laie tun, warum ist das nicht zumutbar}

  Empfehlung: {Welche bedienbare Alternative — z. B. vorhandene Suchkomponente wiederverwenden, Dropdown mit Klartext, Autovervollständigung}

## Geprüfte Interaktionen

Liste der aus der Anforderung geprüften Benutzerinteraktionen:
- {Interaktion} → {Befund vorhanden / unauffällig}

## Geprüfte Dateien

Liste aller geprüften UI-Dateien:
- `src/...`
```

---

## Hinweise

- Nur die im aktuellen Branch geänderten und neu erstellten UI-Dateien prüfen.
- Maßstab ist immer die Kundenanforderung: Es geht um Bedienbarkeit der *geforderten* Aufgaben, nicht um ein vollständiges UX-Gutachten oder Stilpräferenzen.
- Jeden Befund so konkret formulieren, dass der Implementierungsagent ohne Rückfrage eine bedienbare Lösung bauen kann.
- Das Review-Ergebnis muss maschinell lesbar sein: Der Status in der ersten Zeile von „## Ergebnis" ist entweder exakt `Keine Befunde` oder `Befunde vorhanden`.
