# Umsetzungsplan: UI-Korrekturen — Kontextmenü und Drag & Drop im ApplicationGroupTree

## Übersicht

Die bestehenden Blazor-Komponenten `ApplicationContextMenu`, `ApplicationGroupContextMenu`, `ApplicationGroupTree` und `CollapsibleSection` werden um fehlende CSS-Regeln und kleine Markup-Korrekturen ergänzt, um sechs identifizierte Defekte zu beheben: Icon-Qualität, Overlay-Positionierung, Kontextmenü-Funktionalität, Drag-&-Drop-Ausführung und visuelles Feedback, Inline-Ausrichtung des Zahnrad-Icons sowie Hover-/Fokus-gesteuerte Sichtbarkeit. Die Änderungen betreffen ausschließlich UI-Schicht und CSS — kein Datenmodell, keine Repository-Logik, keine Migrationen.

---

## Programmabläufe

### (1) Kontextmenü öffnen und Aktion auslösen

1. Benutzer bewegt Maus über eine `.tree-leaf`-Zeile oder setzt Tastaturfokus auf den Toggle-Button — CSS-Regel macht `.context-menu-toggle` sichtbar.
2. Benutzer klickt den Toggle-Button — `ToggleMenu` setzt `_isOpen = true`.
3. Das `.context-menu-dropdown`-Panel erscheint via `@if (_isOpen)` — positioniert als `position: absolute` relativ zum `.context-menu-container` (`position: relative`).
4. Benutzer klickt einen Menü-Eintrag (z. B. „Bearbeiten") — der `@onclick`-Handler des Buttons wird ausgelöst, weil das Dropdown einen höheren `z-index` hat als das Overlay.
5. Der Handler ruft `EditRequested` (bzw. `DeleteRequested` oder `RemoveFromGroupRequested`) auf — setzt `_isOpen = false` und löst den jeweiligen `EventCallback` aus.
6. Klick außerhalb des Dropdowns trifft das `.context-menu-overlay` — `_isOpen` wird auf `false` gesetzt.

Beteiligte Klassen/Komponenten: `ApplicationContextMenu`, `ApplicationGroupContextMenu`, `app.css`

### (2) Drag & Drop einer Anwendung in eine Gruppe

1. Benutzer beginnt das Ziehen einer Anwendungs-Zeile — `@ondragstart` ruft `OnDragStart(app, e)` auf; `e.DataTransfer.SetData("text", app.Id.ToString())` wird gesetzt; `_draggedApplication` wird gespeichert.
2. Benutzer zieht das Element über einen Gruppen-Container (`CollapsibleSection`) — `@ondragenter` auf dem Gruppen-Container setzt `_dropTargetGroupId = group.Id`; CSS-Klasse `drag-over` wird conditional auf dem Container gerendert.
3. Benutzer verlässt den Gruppen-Container ohne Drop — `@ondragleave` setzt `_dropTargetGroupId = null`; `drag-over`-Klasse entfernt sich.
4. Benutzer lässt das Element auf dem Gruppen-Container los — `@ondrop` wird von `CollapsibleSection.HandleDrop` weitergeleitet; `OnDrop(group.Id)` wird aufgerufen.
5. `OnDrop` setzt `_draggedApplication.ApplicationGroupId = targetGroupId`, ruft `UpdateApplicationAsync` auf, setzt `_draggedApplication = null` und `_dropTargetGroupId = null`, lädt Daten neu.

Beteiligte Klassen/Komponenten: `ApplicationGroupTree`, `CollapsibleSection`, `app.css`

---

## Neue Klassen

Keine.

---

## Änderungen an bestehenden Klassen

### `ApplicationContextMenu` (Blazor-Komponente)

- **Geänderte Methoden:** `ToggleMenu` — unverändert in Logik; Markup wird angepasst (Icon-Zeichen, CSS-Klassenstruktur).
- **Markup-Änderungen:**
  - Zahnrad-Icon: `⚙` (U+2699) wird durch `⚙️` (U+2699 U+FE0F, Emoji-Variante) ersetzt, um konsistente Darstellung ohne neue Abhängigkeit sicherzustellen.
  - Wrapper-Element erhält die CSS-Klasse `context-menu-container` (sofern noch nicht vorhanden), damit `position: relative` aus `app.css` greift.
  - Das `.context-menu-overlay` behält `@onclick="() => _isOpen = false"`, aber der `z-index` des `.context-menu-dropdown` wird über `app.css` so gesetzt, dass er höher ist als der des Overlays.

### `ApplicationGroupContextMenu` (Blazor-Komponente)

- **Markup-Änderungen:** Identisch mit `ApplicationContextMenu` — gleiche Icon-Ersetzung, gleiche Container-Klasse, gleiche CSS-Abhängigkeiten.

### `ApplicationGroupTree` (Blazor-Komponente)

- **Neue Eigenschaften:** `_dropTargetGroupId` (`int?`) — Hält die ID der Gruppe, über der aktuell gezogen wird; `null` wenn kein aktives Drag-over.
- **Geänderte Methoden:** `OnDragStart` — bleibt `void OnDragStart(Application application)` ohne `DragEventArgs`-Parameter. `DataTransfer.SetData` ist in Blazor ohne JS-Interop nicht verfügbar; da JS-Interop nicht im Scope dieser Anforderung liegt, entfällt die Firefox-Kompatibilitätshilfe. Drag & Drop funktioniert in Chromium-Browsern ohne `SetData`.
- **Neue Methoden:** `OnDragEnter(int groupId)` — setzt `_dropTargetGroupId = groupId`.
- **Neue Methoden:** `OnDragLeave` — setzt `_dropTargetGroupId = null`.
- **Geänderte Methoden:** `OnDrop` — setzt zusätzlich `_dropTargetGroupId = null` nach dem Drop.
- **Markup-Änderungen:** `@ondragenter` und `@ondragleave` werden auf Gruppen-Containern ergänzt; CSS-Klasse `drag-over` wird conditional auf dem aktiven Drop-Target gesetzt; Wrapper-Div mit Counter-Logik und `drag-over`-Feedback auch für den „Ohne Gruppe"-Bereich; `.tree-leaf`-Divs erhalten keine strukturellen Änderungen (Inline-Ausrichtung wird über `app.css` geregelt).

### `CollapsibleSection` (Blazor-Komponente)

- **Markup-Änderungen:** `@ondragenter` und `@ondragleave` werden auf dem Root-`<div class="collapsible-section">` ergänzt, sofern ein `OnDragEnter`- bzw. `OnDragLeave`-Callback-Parameter eingeführt wird. Alternativ: Die Events werden direkt in `ApplicationGroupTree` auf dem umgebenden Wrapper-Element registriert, ohne `CollapsibleSection` zu ändern. Bevorzugte Option: Events direkt im `ApplicationGroupTree`-Template auf dem Wrapper-Container um `<CollapsibleSection>` registrieren — vermeidet eine Erweiterung der `CollapsibleSection`-API.

### `app.css` (globale CSS-Datei)

- **Neue Regel `.context-menu-container`:** `position: relative` — Anker für das absolut positionierte Dropdown.
- **Neue Regel `.context-menu-dropdown`:** `position: absolute`, `z-index: 1100` (oberhalb des Overlays mit `z-index: 999`), `background-color`, `border`, `box-shadow`, `min-width` für saubere Darstellung.
- **Neue Regel `.context-menu-toggle`:** `opacity: 0; transition: opacity 0.1s` — standardmäßig unsichtbar.
- **Neue Selektoren für `.context-menu-toggle`-Sichtbarkeit:**
  - `.tree-leaf:hover .context-menu-toggle`
  - `.tree-leaf:focus-within .context-menu-toggle`
  - `.collapsible-section > .d-flex:hover .context-menu-toggle`
  - `.collapsible-section > .d-flex:focus-within .context-menu-toggle`
  - Alle setzen `opacity: 1`.
- **Neue Regel `.tree-leaf`:** `display: flex; align-items: center; justify-content: space-between` — sichert Inline-Ausrichtung von Anwendungsname und Zahnrad-Icon.
- **Neue Regel `.drag-over`:** Visueller Hinweis auf aktives Drop-Target, z. B. `outline: 2px dashed #0d6efd; background-color: rgba(13, 110, 253, 0.05)`.

---

## Datenbankmigrationen

Keine.

---

## Validierungsregeln

Keine.

---

## Konfigurationsänderungen

Keine.

---

## Seiteneffekte und Risiken

- **`.context-menu-dropdown` z-index:** Der neue `z-index: 1100` für `.context-menu-dropdown` muss auf Konflikte mit anderen Overlay-Elementen der Anwendung geprüft werden (z. B. modale Dialoge, Bootstrap-Dropdowns). Bootstrap-Modals verwenden typischerweise `z-index: 1055`, sodass `1100` darüber liegt — dies kann dazu führen, dass Kontextmenüs über geöffneten Modals erscheinen. Empfehlung: `z-index: 1050` testen; falls Konflikte auftreten, den Wert auf `1000` reduzieren (bleibt über dem Overlay mit `999`).
- **`:focus-within`-Sichtbarkeit bei geöffnetem Menü:** Wenn das Dropdown geöffnet ist und der Fokus auf einem Menü-Button liegt, bleibt das Zahnrad-Icon via `:focus-within` sichtbar — dies ist das gewünschte Verhalten. Wenn der Benutzer Tab drückt und den Fokus aus dem Container bewegt, wird das Icon wieder unsichtbar; ob dies akzeptabel ist, hängt von der gewünschten UX ab.
- **Drag & Drop `dataTransfer.SetData` und `DragEventArgs`:** Die Signaturänderung von `OnDragStart` (neue `DragEventArgs e`-Parameter) ändert den Lambda-Ausdruck im Template von `() => OnDragStart(app)` auf `(e) => OnDragStart(app, e)`. Falls andere Stellen im Template dieselbe Methode referenzieren, müssen diese ebenfalls angepasst werden.
- **Scoped CSS:** Die neuen Regeln werden in `app.css` (global) abgelegt, nicht in komponentenspezifischen `.razor.css`-Dateien. Das bedeutet, dass die Klassen `.context-menu-toggle`, `.context-menu-dropdown`, `.tree-leaf` und `.drag-over` potenziell auf andere Komponenten wirken, die dieselben CSS-Klassen verwenden. Da diese Klassen bisher nicht in `app.css` definiert sind, ist das Risiko gering.

---

## Umsetzungsreihenfolge

1. **`app.css` ergänzen** — alle neuen CSS-Regeln hinzufügen (`.context-menu-container`, `.context-menu-dropdown`, `.context-menu-toggle`, `.tree-leaf`, `.drag-over`, Sichtbarkeits-Selektoren). Kann als erstes erfolgen, da keine Abhängigkeiten zu Razor-Änderungen bestehen.
2. **`ApplicationContextMenu.razor` korrigieren** — Icon ersetzen (`⚙️`), sicherstellen dass Wrapper-Element die Klasse `context-menu-container` trägt; keine Logikänderungen.
3. **`ApplicationGroupContextMenu.razor` korrigieren** — identische Änderungen wie Schritt 2.
4. **`ApplicationGroupTree.razor` korrigieren** — `_dropTargetGroupId`-Feld hinzufügen; `OnDragStart`-Signatur auf `DragEventArgs` umstellen; `OnDragEnter`/`OnDragLeave`-Handler hinzufügen; `@ondragstart`-Lambda im Template anpassen; Wrapper-Container um `<CollapsibleSection>` mit `@ondragenter`/`@ondragleave` versehen; `drag-over`-Klasse conditional setzen; `OnDrop` um Reset von `_dropTargetGroupId` ergänzen.
5. **Manuellen Integrationstest durchführen** — Kontextmenü-Öffnen und Aktionen auslösen, Drag & Drop in verschiedenen Browsern (Chrome, Firefox), Hover-/Fokus-Sichtbarkeit der Icons.

---

## Tests

### Neue Tests

Keine neuen automatisierten Tests erforderlich. Die betroffenen Komponenten haben keine bestehende UI-Test-Abdeckung, und die Korrekturen sind rein visueller und ereignisbasierter Natur, die mit Blazor-Komponenten-Unit-Tests nur schwer abzudecken sind. Manueller Test ist ausreichend (siehe Schritt 5 der Umsetzungsreihenfolge).

### Betroffene bestehende Tests

Keine. Die Änderungen betreffen ausschließlich Razor-Markup, CSS und einen Methodenparameter in `ApplicationGroupTree`. Die bestehenden Integrationstests in `ApplicationRepositoryIntegrationTests` testen Repository-Logik und sind von diesen Änderungen nicht betroffen.

---

## Offene Punkte

Keine — alle Punkte wurden abgestimmt:

1. **Icon:** Emoji-Variante `⚙️` wird verwendet.
2. **z-index:** `.context-menu-dropdown` erhält `z-index: 1000` (über dem Overlay mit `999`, unterhalb von Bootstrap-Modals).
3. **Dragenter/Dragleave:** Wrapper-Ansatz — Events direkt auf einem Wrapper-`<div>` im `ApplicationGroupTree`-Template, keine Änderung an `CollapsibleSection`.
4. **Icon-Sichtbarkeit bei geöffnetem Menü:** Das Zahnrad-Icon muss sichtbar bleiben, solange das Dropdown offen ist. Umsetzung: Conditional CSS-Klasse `menu-open` auf dem Container, wenn `_isOpen = true`; Selektor `.context-menu-container.menu-open .context-menu-toggle { opacity: 1 }` in `app.css`.
