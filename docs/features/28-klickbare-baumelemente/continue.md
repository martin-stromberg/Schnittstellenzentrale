# Offene Aufgaben

Erstellt am: 2026-05-28
Abbruchgrund: Maximale Iterationsanzahl (3) erreicht

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

Keine — Plan ist vollständig umgesetzt.

## Code-Review-Befunde

- [ ] **activity-log-panel.js:25 — Listener-Leak bei schnellen Dock↔Overlay-Wechseln (PLAUSIBLE)**: Bei schnellen Wechseln kann die alte Handle-JS-Referenz aus `_listenerRegistry` verschwinden, bevor `destroy()` sie aufräumen kann → `document.mousemove`/`mouseup`-Listener akkumulieren.
- [ ] **MainLayout.razor:40 — `_activityLogPanelHeight` nach JS-Resize veraltet (Known Limitation, dokumentiert)**: `padding-bottom` auf `<article>` weicht nach Drag-Resize von der tatsächlichen Panel-Höhe ab. Bereits als Kommentar dokumentiert.
- [ ] **EndpointScriptRunner.cs:206 — `Task.Run(...).GetAwaiter().GetResult()` blockiert Thread-Pool (Known Limitation, dokumentiert)**: Potenzielle Thread-Pool-Erschöpfung unter Last. Bereits als Kommentar dokumentiert.
- [ ] **EndpointExecutionService.cs:330 — `BuildMaskedDetails` case-sensitiv (PLAUSIBLE)**: `string.Replace` ist case-sensitiv → maskierte Variablenwerte werden nicht redaktiert, wenn der Server sie in abweichender Schreibweise zurückgibt.
