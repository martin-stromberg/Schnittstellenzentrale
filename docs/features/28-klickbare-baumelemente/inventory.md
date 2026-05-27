# Bestandsaufnahme: Klickbare Baumelemente

Analysiert wurden die UI-Komponenten `CollapsibleSection` und `ApplicationGroupTree` sowie die zugehörigen Datenmodelle und Tests, bezogen auf die Anforderung, Baumknoten durch Klick auf den Beschriftungstext auf- und zuzuklappen.

## Zusammenfassung

- `CollapsibleSection` besitzt eine private Methode `Toggle`, die den Kollaps-Zustand invertiert. Sie wird ausschliesslich vom Chevron-Button (`<button class="sz-tree-chevron-btn">`) ausgelöst. Der `<span class="sz-tree-node-text">` trägt **keinen** Click-Handler.
- `ApplicationGroupTree` verwaltet den Aufklapp-Zustand von Anwendungen in `_expandedApplicationIds` (`HashSet<int>`) und die zugehörige Methode `ToggleApplicationExpanded`. Der `<button class="sz-tree-item-btn">` im `RenderApplication`-Fragment ruft ausschliesslich `SelectApplication` auf, nicht `ToggleApplicationExpanded`.
- Für `EndpointGroup`-Knoten (`RenderEndpointGroup`) existiert **kein** Kollaps-Zustand. Der `<span class="sz-tree-item-label">` hat keinen Click-Handler. Der `<div class="sz-tree-children">` wird bedingungslos gerendert — Endpunktordner sind immer aufgeklappt.
- Es existieren weder Unit-Tests für `CollapsibleSection` noch für `ApplicationGroupTree`. Die bestehenden Playwright-Tests decken kein Auf-/Zuklapp-Verhalten ab.
- `TitleActions` in `CollapsibleSection` (Zahnrad-Menü, Context Menu) wird als `RenderFragment`-Parameter übergeben und neben dem Titeltext in der Zeile gerendert; ein zukünftiger Titelklick-Handler muss Klicks auf diesen Bereich ausschliessen.

## Details

- [Datenmodell](inventory/models.md)
- [Logik](inventory/logic.md)
- [Tests](inventory/tests.md)
