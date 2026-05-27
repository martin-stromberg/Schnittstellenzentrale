# Umsetzungsplan: Klickbare Baumelemente

## Übersicht

Der Navigationsbaum (`ApplicationGroupTree`) soll um klickbare Titelzeilen für `ApplicationGroup`-, `Application`- und `EndpointGroup`-Knoten erweitert werden. Aktuell reagiert nur der Chevron-Button auf das Auf-/Zuklappen; nach der Umsetzung löst ein Klick auf den Beschriftungstext dasselbe Verhalten aus. Für `EndpointGroup`-Knoten wird zusätzlich ein bisher nicht vorhandener Kollaps-Zustand sowie ein Chevron-Button eingeführt; Endpunktordner sind initial zugeklappt.

---

## Programmabläufe

### Titelklick klappt `ApplicationGroup`-Knoten auf/zu

1. Der Nutzer klickt auf `<span class="sz-tree-node-text">` innerhalb von `CollapsibleSection`.
2. Der neue `@onclick`-Handler auf dem `<span>` ruft `Toggle` auf.
3. `Toggle` invertiert `_expanded`.
4. Blazor rendert den Baum neu — `ChildContent` wird ein- oder ausgeblendet.

Beteiligte Klassen/Komponenten: `CollapsibleSection`

---

### Titelklick klappt `Application`-Knoten auf/zu und wählt die Anwendung aus

1. Der Nutzer klickt auf `<button class="sz-tree-item-btn">` in `RenderApplication`.
2. Der Handler ruft `SelectAndToggleApplication(app.Id)` auf.
3. `SelectAndToggleApplication` ruft zuerst `ToggleApplicationExpanded(app.Id)` auf — dieser aktualisiert `_expandedApplicationIds` und passt ggf. das SignalR-Hub-Abonnement an.
4. Anschließend ruft `SelectAndToggleApplication` `SelectApplication(app.Id)` auf — dieser löst `OnApplicationSelected` aus.
5. Blazor rendert den Baum neu.

Beteiligte Klassen/Komponenten: `ApplicationGroupTree`

---

### Titelklick klappt `EndpointGroup`-Knoten auf/zu

1. Der Nutzer klickt auf den Chevron-Button oder auf `<span class="sz-tree-item-label">` in `RenderEndpointGroup`.
2. Der `@onclick`-Handler ruft `ToggleEndpointGroupExpanded(group.Id)` auf.
3. `ToggleEndpointGroupExpanded` fügt die ID zu `_expandedEndpointGroupIds` hinzu oder entfernt sie.
4. `RenderEndpointGroup` prüft `_expandedEndpointGroupIds.Contains(group.Id)` und zeigt `sz-tree-children` nur bei aufgeklapptem Zustand.
5. Der Chevron-Button zeigt `bi-chevron-down` (aufgeklappt) oder `bi-chevron-right` (zugeklappt) — analog zu `RenderApplication`.
6. Blazor rendert den Baum neu.

Beteiligte Klassen/Komponenten: `ApplicationGroupTree`

---

## Neue Klassen

Keine.

---

## Änderungen an bestehenden Klassen

### `CollapsibleSection` (Razor-Komponente)

- **Geänderte Methoden:** Der `<span class="sz-tree-node-text">` erhält einen `@onclick="Toggle"`-Handler.

  Bevorzugte Variante: `@onclick="Toggle"` direkt am `<span class="sz-tree-node-text">`, kein Event-Handler an `.sz-tree-row`. Begründung: Minimaler Eingriff, keine Gefahr versehentlicher Trigger durch Klicks auf Icon oder Padding-Bereiche. Da `TitleActions` als separates Geschwisterelement neben dem `<span>` liegt (nicht darin), ist keine Stop-Propagation erforderlich.

---

### `ApplicationGroupTree` (Razor-Komponente)

- **Neuer interner Zustand:** `_expandedEndpointGroupIds` (`HashSet<int>`) — speichert IDs aufgeklappter Endpunktordner; initial leer (entspricht: alle Ordner zugeklappt).

- **Neue Methoden:**
  - `SelectAndToggleApplication(int appId)` (`private async Task`) — Ruft `ToggleApplicationExpanded(appId)` und danach `SelectApplication(appId)` auf; wird als Handler am Anwendungsname-Button eingetragen.
  - `ToggleEndpointGroupExpanded(int endpointGroupId)` (`private void`) — Fügt `endpointGroupId` zu `_expandedEndpointGroupIds` hinzu oder entfernt sie.

- **Geänderte Methoden:**
  - `RenderApplication`: Der `<button class="sz-tree-item-btn">` wird auf den kombinierten Handler `SelectAndToggleApplication(app.Id)` umgestellt (statt nur `SelectApplication(app.Id)`).
  - `RenderEndpointGroup`: Der `<span class="sz-tree-item-label">` erhält einen `@onclick`-Handler, der `ToggleEndpointGroupExpanded(group.Id)` aufruft. Zusätzlich wird ein Chevron-Button (`bi-chevron-down`/`bi-chevron-right`) ergänzt, der ebenfalls `ToggleEndpointGroupExpanded(group.Id)` aufruft. Der `<div class="sz-tree-children">` wird durch eine Bedingung (`_expandedEndpointGroupIds.Contains(group.Id)`) gesteuert, statt bedingungslos gerendert zu werden.

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

- **`RenderEndpointGroup` — initialer Zustand zugeklappt:** Durch die Einführung von `_expandedEndpointGroupIds` werden Endpunktordner beim ersten Laden standardmäßig zugeklappt dargestellt. Das ist eine sichtbare Verhaltensänderung gegenüber dem aktuellen Stand (immer aufgeklappt). Bei Reload oder Datenneuladung (`LoadDataAsync`, `ReloadApplicationDataAsync`, `OnModeChanged`) werden alle aufgeklappten Zustände zurückgesetzt — analog zum bestehenden Verhalten für `_expandedApplicationIds`.

- **`RenderApplication` — kombinierter Klick:** Bisher wurde ein Klick auf den Anwendungsname-Button nur als Auswahl interpretiert. Nach der Änderung klappt er zusätzlich auf/zu. Bestehende Playwright-Tests, die auf den Anwendungsname-Button klicken und anschließend einen geöffneten Dialog oder eine Auswahl prüfen, könnten durch unerwartetes Auf-/Zuklappen beeinflusst werden — dies ist zu prüfen.

- **`CollapsibleSection` — `TitleActions` Klick-Isolation:** Die `TitleActions` befinden sich als separates Geschwisterelement neben dem `<span class="sz-tree-node-text">` innerhalb von `.sz-tree-row` — da der Handler nur am `<span>` hängt, ist keine Stop-Propagation erforderlich.

---

## Umsetzungsreihenfolge

1. `CollapsibleSection`: `@onclick="Toggle"` am `<span class="sz-tree-node-text">` ergänzen.
2. `ApplicationGroupTree`: Neuen internen Zustand `_expandedEndpointGroupIds` (`HashSet<int>`) hinzufügen.
3. `ApplicationGroupTree`: Methode `ToggleEndpointGroupExpanded(int)` implementieren.
4. `ApplicationGroupTree`: `RenderEndpointGroup` anpassen — Chevron-Button ergänzen (`bi-chevron-down`/`bi-chevron-right`), `@onclick`-Handler am `<span class="sz-tree-item-label">` und am Chevron-Button eintragen, bedingte Darstellung der Kindknoten auf Basis von `_expandedEndpointGroupIds` umstellen.
5. `ApplicationGroupTree`: Methode `SelectAndToggleApplication(int)` implementieren.
6. `ApplicationGroupTree`: `RenderApplication` anpassen — Klick-Handler des `<button class="sz-tree-item-btn">` auf `SelectAndToggleApplication` umstellen.
7. Playwright-Testklasse `TreeCollapseTests` anlegen und neue Tests implementieren.

---

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|---|---|---|
| `ClickApplicationGroupTitle_TogglesCollapse` | `TreeCollapseTests` | Ein Klick auf den `ApplicationGroup`-Titeltext klappt den Knoten zu; ein zweiter Klick klappt ihn wieder auf. |
| `ClickApplicationName_ExpandsApplication` | `TreeCollapseTests` | Ein Klick auf den Anwendungsnamen klappt die Anwendung auf und zeigt untergeordnete Elemente an. |
| `ClickApplicationName_SelectsApplication` | `TreeCollapseTests` | Ein Klick auf den Anwendungsnamen selektiert die Anwendung (prüfbar durch Seiteninhalt oder UI-Zustand). |
| `ClickApplicationName_CollapsesExpandedApplication` | `TreeCollapseTests` | Ein Klick auf einen bereits aufgeklappten Anwendungsnamen klappt die Anwendung wieder zu. |
| `ClickEndpointGroupName_TogglesCollapse` | `TreeCollapseTests` | Ein Klick auf den Endpunktordner-Namen klappt den Ordner auf; ein zweiter Klick klappt ihn wieder zu. |
| `ClickEndpointGroupChevron_TogglesCollapse` | `TreeCollapseTests` | Ein Klick auf den Chevron-Button des Endpunktordners klappt den Ordner auf; ein zweiter Klick klappt ihn wieder zu. |
| `EndpointGroupInitiallyCollapsed` | `TreeCollapseTests` | Endpunktordner sind beim Laden des Baums initial zugeklappt. |

### Betroffene bestehende Tests

| Test / Testklasse | Grund der Anpassung |
|---|---|
| `CreateApplication_AppearsInTree` (`ApplicationCrudTests`) | Prüfen, ob der Test nach der Änderung noch fehlerfrei läuft: Nach dem Klick auf den Anwendungsnamen löst der Button jetzt zusätzlich `ToggleApplicationExpanded` aus. Kein Anpassungsbedarf erwartet, aber zu verifizieren. |
| `EditApplication_UpdatesNameInTree` (`ApplicationCrudTests`) | Wie oben — zu prüfen, ob der Test weiterhin stabil ist. |
| `DeleteApplication_DisappearsFromTree` (`ApplicationCrudTests`) | Wie oben. |

---

## Offene Punkte

Keine.
