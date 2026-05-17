# Anforderung: UI-Korrekturen — Kontextmenü und Drag & Drop im ApplicationGroupTree

## Fachliche Zusammenfassung

Die bestehende Implementierung des Features „Bearbeitung der Anwendungen" im `ApplicationGroupTree` enthält mehrere visuelle und funktionale Defekte, die korrigiert werden müssen. Betroffen sind die Kontextmenü-Komponenten `ApplicationContextMenu` und `ApplicationGroupContextMenu` (Zahnrad-Icon-Qualität, Overlay-Darstellung, fehlende Aktionsauslösung) sowie die Drag-&-Drop-Logik in `ApplicationGroupTree` (fehlende Drop-Ausführung, fehlendes visuelles Feedback). Zusätzlich muss das Zahnrad-Icon der `Application`-Zeile korrekt inline positioniert und die Sichtbarkeit beider Zahnrad-Symbole auf Hover- und Fokuszustand eingeschränkt werden.

---

## Betroffene Klassen und Komponenten

### UI-Komponenten (Blazor) — zu korrigieren

| Komponente | Defekt / Korrektur |
|---|---|
| `ApplicationContextMenu` | (1) Icon-Qualität: Zahnrad-Zeichen durch ein sauber darstellbares Unicode-Symbol ersetzen. (2) Overlay-Darstellung: Dropdown als korrekt positioniertes Overlay-Panel direkt am Zahnrad-Symbol rendern. (3) Funktionalität: Menüaktionen „Bearbeiten" und „Löschen" werden nicht ausgelöst — Event-Propagation und Overlay-Interaktionsreihenfolge korrigieren. (5) Position: Icon bricht in eine neue Zeile um — Inline-Ausrichtung sicherstellen. (6) Sichtbarkeit: Icon nur bei Hover oder Tastaturfokus einblenden. |
| `ApplicationGroupContextMenu` | (1) Icon-Qualität: Gleiches Symbol wie `ApplicationContextMenu`. (2) Overlay-Darstellung: Analog zu `ApplicationContextMenu`. (3) Funktionalität: Menüaktionen werden nicht ausgelöst — gleiche Ursache wie in `ApplicationContextMenu`. (6) Sichtbarkeit: Icon nur bei Hover oder Tastaturfokus einblenden. |
| `ApplicationGroupTree` | (4) Drag & Drop — Drop-Ausführung: `ondrop`-Handler greift nicht, Anwendung wird nicht in Zielgruppe verschoben. Ursache vermutlich fehlende `dataTransfer`-Nutzung oder fehlendes `preventDefault` auf dem Drop-Target. (4) Drag & Drop — visuelles Feedback: Während des Ziehens wird kein Drop-Target hervorgehoben; CSS-Klasse (z. B. `drag-over`) muss über `ondragenter`/`ondragleave`-Events gesetzt und entfernt werden. |
| `CollapsibleSection` | (4) Drop-Handling: Prüfen, ob `ondrop`-Event korrekt an den `OnDrop`-Callback weitergereicht wird; ggf. `preventDefault` für `ondragover` auf dem Container ergänzen, damit der Browser den Drop zulässt. |

### CSS (`app.css` oder Scoped CSS)

| Regel | Zweck |
|---|---|
| `.context-menu-dropdown` (neu/korrigiert) | Position `absolute`, korrekte `z-index`-Ebene oberhalb der Seitenleiste, sauberer visueller Stil (Hintergrund, Schatten, Rahmen). |
| `.context-menu-toggle` (Sichtbarkeit) | Standardmäßig unsichtbar (`opacity: 0` oder `visibility: hidden`); bei `:hover` auf dem Eltern-Container sowie bei `:focus-within` sichtbar (`opacity: 1`). |
| `.tree-leaf`, `.collapsible-section` (Hover-Container) | Müssen als Hover-Kontext für die Sichtbarkeitsregel dienen; ggf. `position: relative` setzen, damit Hover korrekt weitergegeben wird. |
| `.drag-over` (neu) | Visueller Hinweis auf das aktive Drop-Target (z. B. `outline` oder `background-color`), wird per JavaScript-Event-Handler gesetzt und entfernt. |

---

## Implementierungsansatz

### (1) Zahnrad-Icon-Qualität
Das aktuelle Zeichen `⚙` (U+2699) ist bereits Unicode, stellt sich jedoch je nach Schriftart und Browser-Rendering unterschiedlich dar. Alternativen: `⚙️` (U+2699 + U+FE0F, Emoji-Variante mit Farbdarstellung) oder ein SVG-Icon aus Bootstrap Icons (z. B. `bi-gear`). *Annahme: Bootstrap Icons sind bereits im Projekt verfügbar, da Bootstrap verwendet wird; falls nicht, ist das Emoji-Variante-Suffix der einfachste Fix ohne neue Abhängigkeit.*

### (2) Kontextmenü als Overlay
Das `.context-menu-dropdown` benötigt `position: absolute` relativ zum umgebenden `.context-menu-container` (`position: relative`). Der Container muss korrekt im Dokumentfluss positioniert sein, damit das Dropdown neben dem Icon erscheint und nicht vom Eltern-Container abgeschnitten wird (`overflow: visible` auf Vorfahren prüfen).

### (3) Kontextmenü-Funktionalität
Das Overlay-`<div>` mit `@onclick="() => _isOpen = false"` fängt Klicks auf die Menü-Schaltflächen ab, bevor deren `@onclick`-Handler ausgeführt wird, wenn das Dropdown über dem Overlay liegt (z-Index-Problem) oder das Event-Bubbling die falsche Reihenfolge erzeugt. Korrektur: Das Dropdown muss einen höheren `z-index` als das Overlay haben, oder das Overlay darf nicht zwischen Dropdown und Benutzer liegen. Alternative: Klick-außerhalb-Erkennung über einen JavaScript-Interop-Handler statt eines fixen Overlays.

### (4) Drag & Drop
- **Drop-Ausführung:** `CollapsibleSection` leitet `ondrop` über `HandleDrop` weiter. Zu prüfen, ob `e.preventDefault()` auf `ondragover` im richtigen Scope gesetzt ist. In `ApplicationGroupTree` fehlt möglicherweise das Setzen der `dataTransfer`-Daten in `ondragstart` (zwingend erforderlich in Firefox). Ergänzung: `@ondragstart` erhält `DragEventArgs` und ruft `e.DataTransfer.SetData("text", app.Id.ToString())` auf.
- **Visuelles Feedback:** `ApplicationGroupTree` verwaltet eine `_dropTargetGroupId`-Variable (analog zu `_draggedApplication`). `ondragenter` auf Gruppe/`CollapsibleSection` setzt den Wert; `ondragleave` und `ondrop` setzen ihn zurück. Die CSS-Klasse `drag-over` wird conditional auf dem betreffenden Gruppen-Container gerendert.

### (5) Zahnrad-Icon-Position (Application)
Das `<ApplicationContextMenu>`-Element steht im Template hinter dem `<button class="btn btn-link">`, aber das `.tree-leaf`-Div fehlt möglicherweise `display: flex` und `align-items: center`. Korrektur in `app.css`: `.tree-leaf { display: flex; align-items: center; justify-content: space-between; }` — sicherstellt, dass Name und Icon in einer Zeile bleiben.

### (6) Sichtbarkeit der Zahnrad-Symbole (Hover/Fokus)
CSS-Lösung ohne JavaScript-Interop:
```css
.context-menu-toggle {
    opacity: 0;
    transition: opacity 0.1s;
}
.tree-leaf:hover .context-menu-toggle,
.tree-leaf:focus-within .context-menu-toggle,
.collapsible-section > .d-flex:hover .context-menu-toggle,
.collapsible-section > .d-flex:focus-within .context-menu-toggle {
    opacity: 1;
}
```
Wenn das Menü geöffnet ist (`_isOpen = true`), muss das Icon ebenfalls sichtbar bleiben — dies wird durch `:focus-within` auf dem Container abgedeckt, sofern der Fokus beim Öffnen auf dem Dropdown-Button liegt.

---

## Konfiguration

Kein Konfigurationsbedarf. Alle Korrekturen betreffen ausschließlich UI-Komponenten und CSS.

---

## Offene Fragen

1. **Icon-Ersatz:** Soll das Zahnrad durch ein SVG-Icon aus Bootstrap Icons (`bi-gear`) ersetzt werden (erfordert Prüfung, ob Bootstrap Icons im Projekt eingebunden sind), oder genügt die Emoji-Variante `⚙️` (U+2699 U+FE0F)? Dies bestimmt, ob eine neue Abhängigkeit einzuführen ist.

2. **Klick-außerhalb-Erkennung:** Soll die bestehende Overlay-Methode (unsichtbares `<div>` mit `@onclick`) beibehalten werden (einfacher, aber anfällig für z-Index-Konflikte), oder soll auf JavaScript-Interop (`document.addEventListener('click', ...)`) umgestellt werden (robuster, aber zusätzlicher JS-Code)?

3. **Drag & Drop in Firefox:** Erfordert Firefox zwingend `dataTransfer.setData()` in `ondragstart`. Soll eine `DragEventArgs`-basierte Implementierung eingeführt werden, oder wird Firefox nicht als Zielbrowser unterstützt?

4. **Sichtbarkeit bei geöffnetem Menü:** Wenn das Dropdown geöffnet ist, muss das Zahnrad-Icon sichtbar bleiben. Reicht die `:focus-within`-CSS-Regel, oder muss zusätzlich eine CSS-Klasse `is-open` programmatisch gesetzt werden, wenn `_isOpen = true`?

5. **Scoped CSS vs. globales CSS:** Sollen die neuen Regeln für `.context-menu-toggle`, `.drag-over` und `.tree-leaf` in `app.css` (global) oder in komponentenspezifischen `.razor.css`-Dateien (`ApplicationContextMenu.razor.css`, `ApplicationGroupTree.razor.css`) abgelegt werden? Letzteres vermeidet unbeabsichtigte Seiteneffekte, erfordert aber neue Dateien.
