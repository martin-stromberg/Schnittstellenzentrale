# Plan gegen Anforderung prüfen

Du bekommst eine übersetzte Anforderung (`requirement.md`), eine Bestandsaufnahme (`inventory.md` inkl. Detaildokumente) und einen Umsetzungsplan (`plan.md`). Prüfe, ob der Plan die Anforderung vollständig abdeckt, bevor der Planungscommit erstellt wird. Für Benutzerflüsse sind E2E-Tests der primäre Funktionsnachweis.

**Ziel:** Lücken zwischen Anforderung und Plan aufdecken — keine Implementierung, keine Codeprüfung, keine Korrektur des Plans.

---

## Schritt 1: Eingaben einlesen

Lies folgende Dateien vollständig:

- `docs/features/{branchname}/requirement.md` — übersetzte Anforderung mit Akzeptanzkriterien
- `docs/features/{branchname}/inventory.md` — Übersicht der Bestandsaufnahme
- Alle verlinkten Detaildokumente unter `docs/features/{branchname}/inventory/`
- `docs/features/{branchname}/plan.md` — zu prüfender Umsetzungsplan

## Schritt 2: Prüfbasis ermitteln

Erstelle intern eine Liste aller prüfpflichtigen Punkte:

- Alle Akzeptanzkriterien aus `requirement.md`
- Alle fachlichen Muss-Anforderungen und expliziten Nicht-Anforderungen
- Den Abschnitt `Fachlicher Scope` aus `requirement.md`: Verarbeitungseinheit, auslösende Ereignisse, erwartete Wirkung, Umgang mit fehlenden Objekten, Scope-Ausschlüsse
- Alle Benutzerinteraktionen, UI-Flows, Navigationen, Buttons, Fokus- oder Ribbon-Aktionen
- Alle End-to-End-Abläufe, über die ein Anwender die neue oder geänderte Funktion tatsächlich nutzt
- Alle Akzeptanzszenarien aus `plan.md`, insbesondere den vollständigen Happy Path und Szenarien mit fehlenden Ausgangsobjekten
- Alle Wartesituationen, asynchronen Prozesse und externen Abhängigkeiten samt Zuständen, Übergängen und Wiederaufnahmewegen
- Alle Daten-Sichtbarkeits-, Berechtigungs- und Filterregeln
- Alle Validierungs- und Fehlerfälle
- Alle bestehenden Tests, die laut Bestandsaufnahme betroffen sein können
- Alle Testdaten, Fixtures oder Hilfsmethoden, die für die geplanten Tests notwendig sind

## Schritt 3: Planvollständigkeit prüfen

Prüfe für jeden Punkt aus Schritt 2:

- Ist die Umsetzung im Plan konkret beschrieben?
- Ist die Änderung in der Umsetzungsreihenfolge als ausführbarer Schritt enthalten?
- Sind notwendige Voraussetzungen im Plan genannt oder als früherer Schritt angelegt?
- Sind Seiteneffekte und Risiken benannt, wenn bestehende Abläufe betroffen sind?
- Ist der Abschnitt `Scope-Klärung` vollständig gefüllt (Verarbeitungseinheit, auslösende Ereignisse, erwartete Wirkung, Umgang mit fehlenden Objekten)?
- Ist jede Einschränkung gegenüber einer naheliegenden Nutzererwartung als Scope-Ausschluss mit Begründung dokumentiert? Tragen alle Ausschlüsse einen Status (`offen`/`bestätigt`)?
- Enthält der Abschnitt `Akzeptanzszenarien` mindestens einen vollständigen Happy Path vom externen oder Nutzer-Auslöser bis zur sichtbaren Wirkung?
- Benennt jedes Akzeptanzszenario den relevanten Ausgangszustand — insbesondere den Fall, dass erwartete Objekte, Konfigurationen oder Sitzungen noch nicht existieren?
- Sind für Wartesituationen, asynchrone Prozesse und externe Abhängigkeiten Zustände, Übergänge und wirksame Wiederaufnahmewege definiert?
- Ist für jede sichtbare Benutzeraktion festgelegt, dass sie den beschriebenen nächsten Schritt tatsächlich auslösen kann — bzw. wie die Blockade dem Anwender eindeutig erklärt wird?

## Schritt 4: Testvollständigkeit prüfen

Prüfe, ob der Plan die Testanforderungen vollständig ermittelt:

- Zu jedem Akzeptanzkriterium gibt es mindestens einen konkreten Testnachweis im Abschnitt `Tests` oder `E2E-Tests`.
- Für jedes Akzeptanzkriterium, das über UI oder Nutzeraktion erreichbar ist, gibt es einen konkreten E2E-Test. Reine Unit- oder Integrationstests reichen dafür nicht aus.
- Happy-Path-Tests decken das erwartete Normalverhalten ab.
- Negative Tests und Edge-Case-Tests decken ab, was nicht passieren darf.
- Autorisierungs- und Sichtbarkeitstests decken ab, wer etwas sehen oder ausführen darf und was verborgen bleiben muss.
- Bei Daten-Sichtbarkeits-, Berechtigungs- oder Filterlogik gibt es mindestens einen Unit- oder Integrationstest auf Service-/Logikebene zusätzlich zu möglicher E2E-Abdeckung.
- Bei neuen oder geänderten UI-Flows gibt es konkrete E2E-Tests für den exakten Benutzerfluss, inklusive Setup, auslösender Nutzeraktion und sichtbarem Ergebnis.
- Bei Plänen mit Dialogen, Overlays oder Panels sind geometrische Assertions (BoundingBox-Vergleich: Inhalt innerhalb der Dialog-/Container-Grenzen) als E2E-Anforderung geplant.
- Bei Plänen mit externen Werkzeug- oder Protokollschnittstellen (z. B. MCP-Tools) ist die reale Abnahme über echte Protokollaufrufe (z. B. `tools/call`) gegen Seed-/Demo-Daten mit Domain-Randwerten eingeplant.
- Geplante Testdaten und Fixtures bilden die Wertebereiche und Konventionen der Domain ab (vorzeichenbehaftete Felder mit negativen Werten, Grenzwerte, volle Enum-Abdeckung) — nicht nur Implementierer-Annahmen.
- Geplante Validierungen an externen Schnittstellen beschränken sich auf Syntax und Pflichtfelder; keine Regel zieht den Wertebereich enger als der Domain-Vertrag.
- Sichtbare Fehlerfälle und Berechtigungs-/Sichtbarkeitsregeln sind als E2E-Szenarien geplant, sofern sie über die UI ausgelöst oder beobachtet werden können.
- Falls der Plan keine E2E-Tests vorsieht, enthält er eine nachvollziehbare Begründung, warum die Anforderung keinen Benutzerfluss berührt.
- Betroffene bestehende Tests sind vollständig genannt.
- Notwendige Testdaten, Fixtures und Hilfsmethoden sind vollständig genannt.

## Schritt 5: Ergebnis speichern

Speichere das Ergebnis als `docs/features/{branchname}/plan-check.md` mit exakt dieser Struktur:

```
# Plan-Gegenprüfung

## Ergebnis

**Status:** Plan vollständig | Plan lückenhaft

## Abgleich Akzeptanzkriterien

| Akzeptanzkriterium | Umsetzung im Plan | Testnachweis im Plan | Status |
|--------------------|-------------------|----------------------|--------|
| ... | ... | ... | Abgedeckt / Lücke |

## Fehlende oder unvollständige Testanforderungen

Nur ausfüllen, wenn Status `Plan lückenhaft`:

- [ ] {konkrete fehlende Testanforderung mit Bezug zum Akzeptanzkriterium}

## E2E-Abdeckung

| Benutzerfluss / Akzeptanzkriterium | Geplanter E2E-Test | Status |
|------------------------------------|--------------------|--------|
| ... | ... | Abgedeckt / Lücke / Nicht erforderlich mit Begründung |

## Fehlende oder unvollständige Planbestandteile

Nur ausfüllen, wenn Status `Plan lückenhaft`:

- [ ] {konkrete fehlende Umsetzung, Entscheidung, Voraussetzung oder Risikoabdeckung}

## Hinweise

Optionale Hinweise zur Nachplanung.
```

---

## Hinweise

- Der Status in `## Ergebnis` ist maschinenlesbar und muss exakt `Plan vollständig` oder `Plan lückenhaft` lauten.
- Eine fehlende Testanforderung macht den Status immer `Plan lückenhaft`, auch wenn die Umsetzungsschritte vollständig wirken.
- Eine fehlende Abdeckung eines Akzeptanzkriteriums macht den Status immer `Plan lückenhaft`.
- Eine fehlende E2E-Abdeckung für einen Benutzerfluss macht den Status immer `Plan lückenhaft`.
- Eine fehlende oder unvollständige `Scope-Klärung` macht den Status immer `Plan lückenhaft`.
- Ein fehlender Abschnitt `Akzeptanzszenarien` oder ein fehlender vollständiger Happy Path macht den Status immer `Plan lückenhaft`.
- Ein Scope-Ausschluss ohne Status oder ohne Begründung macht den Status immer `Plan lückenhaft`.
- Unit- und Integrationstests dürfen fehlende E2E-Tests für Benutzerflüsse nicht kompensieren.
- Prüfe nur die Vollständigkeit des Plans gegen die Anforderung und Bestandsaufnahme. Bewertete Codequalität, Architekturqualität oder Implementierungsstand gehören nicht in dieses Kommando.
- Keine Korrekturen vornehmen — ausschließlich prüfen und dokumentieren.
