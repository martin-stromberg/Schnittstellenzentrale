### Fachliche Zusammenfassung

Im Navigationsbaum (`ApplicationGroupTree`) sollen `ApplicationGroup`-, `Application`- und `EndpointGroup`-Knoten beim Klick auf den Beschriftungstext auf- oder zugeklappt werden. Bisher reagiert nur ein Klick auf das vorangestellte Pfeilsymbol (Chevron-Button) auf diese Aktion. Das neue Verhalten ergänzt die bestehende Klick-Fläche um den gesamten Zeilentitel, sodass ein Klick auf den Namen denselben Effekt hat wie ein Klick auf das Chevron-Symbol.

### Betroffene Klassen und Komponenten

**UI-Komponenten:**
- `CollapsibleSection` — Steuert das Auf-/Zuklappen von `ApplicationGroup`- und (indirekt) anderen Abschnitten über die private Methode `Toggle`. Der `<span class="sz-tree-node-text">` enthält derzeit keinen Click-Handler; dieser muss ergänzt oder der gesamte Titelbereich als klickbares Element umgestaltet werden.
- `ApplicationGroupTree` (Razor-Komponente) — Enthält:
  - `RenderApplication`: Die Anwendungszeile rendert Chevron-Button (ruft `ToggleApplicationExpanded`) und Namens-Button (`sz-tree-item-btn`, ruft `SelectApplication`) separat. Der Namens-Button löst aktuell nur die Auswahl der Anwendung aus, nicht das Auf-/Zuklappen.
  - `RenderEndpointGroup`: Die Endpunktordner-Zeile rendert Chevron-Button und Namensbeschriftung als separate Elemente ohne Kollaps-Funktionalität auf dem Namenslabel (derzeit `<span class="sz-tree-item-label">` ohne Click-Handler).

**Tests:**
- Playwright-Integrationstests für den Navigationsbaum (Testdateien unter `tests/`) — müssen um Szenarien für den Klick auf den Titel ergänzt werden.
- Unit-Tests für `CollapsibleSection` (falls vorhanden).

### Implementierungsansatz

**`CollapsibleSection`:**
- Der `<span class="sz-tree-node-text">` (oder ein umschließendes `<div>`/`<button>`) erhält einen `@onclick`-Handler, der `Toggle` aufruft — analog zum bestehenden Chevron-Button.
- Alternativ kann die gesamte Titelzeile (`.sz-tree-row`) als klickbare Fläche gestaltet werden; dabei ist darauf zu achten, dass Klicks auf `TitleActions` (z. B. das Zahnrad-Menü) nicht versehentlich den Kollaps auslösen (`@onclick:stopPropagation` auf den `TitleActions`-Container).

**`RenderApplication` in `ApplicationGroupTree`:**
- Der bestehende `<button class="sz-tree-item-btn">` mit dem Anwendungsnamen ruft derzeit `SelectApplication(app.Id)` auf. Ein Klick auf ihn soll zusätzlich `ToggleApplicationExpanded(app.Id)` auslösen, oder die beiden Aktionen werden in einem kombinierten Handler zusammengeführt.
- Alternativ: Der Namensbeschriftungsbereich wird zu einem einzigen Click-Bereich kombiniert, der beide Aktionen (Auswahl + Kollaps) auslöst.

**`RenderEndpointGroup` in `ApplicationGroupTree`:**
- Der `<span class="sz-tree-item-label">` mit dem Ordnernamen erhält einen `@onclick`-Handler, der das Auf-/Zuklappen des Ordners steuert. Dazu ist ein privater Zustand für aufgeklappte Endpunktordner (analog zu `_expandedApplicationIds`) in `ApplicationGroupTree` oder eine Erweiterung von `RenderEndpointGroup` notwendig — *Annahme: derzeit werden Endpunktordner immer vollständig aufgeklappt dargestellt, da kein Kollaps-Zustand existiert.*

**Erweiterungspunkte:**
- `CollapsibleSection.Toggle` (bereits vorhanden, nur neue Trigger nötig)
- `ApplicationGroupTree.ToggleApplicationExpanded` (bereits vorhanden, neue Auslöser nötig)
- Neuer Zustand `_expandedEndpointGroupIds` (HashSet analog zu `_expandedApplicationIds`) für Endpunktordner — *Annahme: aktuell kein solcher Zustand vorhanden.*

### Konfiguration

Kein Konfigurationsbedarf. Das Verhalten entspricht einem UX-Standard (klickbarer Titel = Auf-/Zuklappen) und soll für alle Benutzer einheitlich gelten.

### Offene Fragen

1. **Anwendungsknoten — kombinierte Aktion:** Soll ein Klick auf den Anwendungsnamen gleichzeitig die Anwendung auswählen *und* auf-/zuklappen, oder nur auf-/zuklappen (analog zu Gruppen und Ordnern, die keine Auswahlaktion haben)?
2. **Endpunktordner — aktueller Kollaps-Zustand:** Sind Endpunktordner derzeit immer ausgeklappt (kein Zustandsmanagement), oder existiert bereits ein versteckter Kollaps-Mechanismus? Die Antwort bestimmt den Implementierungsaufwand für Endpunktordner.
3. **Klickbereich für `CollapsibleSection`:** Soll nur der Titeltext klickbar sein, oder die gesamte Titelzeile (exklusive des Zahnrad-Menüs)?
