# Anwendungen — Business Rules

## Owner-Zuweisung im Benutzermodus

**Beschreibung:** Im Benutzermodus (`StorageMode.User`) müssen Anwendungen einem konkreten Benutzer gehören, damit die Filterfunktion in `GetApplicationsAsync` nur die eigenen Einträge zurückgibt.

**Bedingungen:** `IStorageModeService.CurrentMode == StorageMode.User`

**Verhalten:**
- `StorageMode.User`: `_model.Owner` wird vor dem Speichern auf `ICurrentUserService.GetCurrentUserName()` gesetzt.
- `StorageMode.Team`: `_model.Owner` bleibt `null`; die Anwendung ist für alle sichtbar.

**Umsetzung:** `ApplicationEditor.SaveAsync` — Zuweisung unmittelbar vor `AddApplicationAsync` bzw. `UpdateApplicationAsync`.

---

## Gegenseitiger Ausschluss der Anlage-Formulare

**Beschreibung:** Da beide Formulare in derselben schmalen Seitenleiste liegen, darf immer nur eines sichtbar sein.

**Verhalten:**
- „Neue Gruppe" angeklickt: `_showGroupEditor = true`, `_showApplicationEditor = false`.
- „Neue Anwendung" angeklickt: `_showApplicationEditor = true`, `_showGroupEditor = false`.

**Umsetzung:** `ApplicationGroupTree.ShowGroupEditor` / `ShowApplicationEditor`.

---

## SignalR-Benachrichtigung nur im Teammodus

**Beschreibung:** Live-Updates via SignalR sind nur im Teammodus sinnvoll, da im Benutzermodus jeder Benutzer ausschließlich eigene Daten sieht.

**Bedingungen:** `IStorageModeService.CurrentMode == StorageMode.Team`

**Verhalten:** Nach jeder persistierenden Operation wird `ISignalRNotificationService.NotifyGroupChangedAsync` bzw. `NotifyApplicationChangedAsync` aufgerufen. Im Benutzermodus entfällt die Benachrichtigung.

**Umsetzung:** Beim Anlegen (Anlage-Modus) wird der `StorageMode` per `X-Storage-Mode`-Header an den REST-Controller übergeben; `ApplicationGroupsController.CreateAsync` und `ApplicationsController.CreateAsync` rufen `ISignalRNotificationService` auf. Bei Bearbeitungs-, Lösch- und Drag-&-Drop-Operationen bleiben `ApplicationEditor.SaveAsync` und alle Handler in `ApplicationGroupTree` zuständig.

---

## Gruppe löschen: Umgang mit enthaltenen Anwendungen

**Beschreibung:** Beim Löschen einer Gruppe, die noch Anwendungen enthält, muss der Anwender explizit entscheiden, was mit ihnen geschehen soll.

**Option 1 — Mitlöschen:**
- Alle Anwendungen werden einzeln per `DeleteApplicationAsync` gelöscht; im Team-Modus wird für jede `NotifyApplicationChangedAsync` ausgelöst. Dann wird die Gruppe per `DeleteGroupAsync` gelöscht.
- Umsetzung: `ApplicationGroupTree.OnDeleteGroupConfirmedAll` via `ProcessGroupApplicationsAsync`. Die Schleife läuft UI-seitig, da kein kaskadierendes Delete auf DB-Ebene konfiguriert ist (`DeleteBehavior.SetNull`).

**Option 2 — Nur Gruppe löschen:**
- `ApplicationGroupId` aller enthaltenen Anwendungen wird auf `null` gesetzt per `UpdateApplicationAsync`; im Team-Modus wird für jede `NotifyApplicationChangedAsync` ausgelöst. Dann wird die Gruppe gelöscht.
- Umsetzung: `ApplicationGroupTree.OnDeleteGroupConfirmedGroupOnly` via `ProcessGroupApplicationsAsync` — das explizite Entkoppeln ist auch dann nötig, wenn `DeleteBehavior.SetNull` auf DB-Ebene konfiguriert ist, damit SignalR-Benachrichtigungen für die einzelnen Anwendungen ausgelöst werden.

---

## Aus Gruppe entfernen: Rollback bei Fehler

**Beschreibung:** Die Aktion ändert `ApplicationGroupId` direkt am In-Memory-Objekt, bevor der Persistenzaufruf erfolgt. Der Ursprungswert wird gesichert, um bei einem Fehler einen inkonsistenten Zustand zu vermeiden.

**Verhalten:**
- Erfolgreich: `ApplicationGroupId = null` bleibt gesetzt; Baum wird neu geladen.
- Fehler: `application.ApplicationGroupId` wird auf `previousGroupId` zurückgesetzt; `_errorMessage` wird angezeigt.

**Umsetzung:** `ApplicationGroupTree.OnRemoveFromGroupRequested`. Analoges Rollback-Muster ist in `OnDrop` implementiert.

---

## Drag & Drop: Rollback bei Fehler

**Beschreibung:** Beim Drag & Drop wird `ApplicationGroupId` vor dem Persistenzaufruf geändert. Schlägt der Aufruf fehl, wird der Ursprungswert wiederhergestellt.

**Verhalten:**
- Erfolgreich: `_draggedApplication` wird auf `null` gesetzt; Baum wird neu geladen.
- Fehler: `_draggedApplication.ApplicationGroupId = previousGroupId`; `_errorMessage` wird angezeigt.

**Umsetzung:** `ApplicationGroupTree.OnDrop`.

---

## Nebenläufigkeitskonflikte (RowVersion)

**Beschreibung:** `Application` und `ApplicationGroup` besitzen ein `RowVersion`-Feld für optimistische Nebenläufigkeitskontrolle. Hat eine andere Instanz den Datensatz zwischen Laden und Speichern geändert, wirft EF Core eine `DbUpdateConcurrencyException`.

**Betroffene Operationen:** `UpdateApplicationAsync` in `ApplicationEditor.SaveAsync` und `UpdateGroupAsync` in `ApplicationGroupTree.OnGroupRenamed`.

**Verhalten:**
- Der Dialog bleibt geöffnet (kein automatisches Schließen).
- Fehlermeldung: „Die Daten wurden zwischenzeitlich geändert. Bitte laden Sie die Seite neu und versuchen Sie es erneut."
- Kein Force-Save (kein Überschreiben der Fremdänderung).

---

## Zahnrad-Icon — Sichtbarkeit bei Hover, Fokus und geöffnetem Menü

**Beschreibung:** Das Zahnrad-Icon soll den Navigationsbaum nicht dauerhaft visuell belasten, muss aber erreichbar sein — auch per Tastatur — und sichtbar bleiben, solange das Menü geöffnet ist.

**Bedingungen:**
- Icon standardmäßig: `opacity: 0`
- Icon sichtbar, wenn: Mauszeiger über `.tree-leaf` oder `.collapsible-section > .d-flex`
- Icon sichtbar, wenn: Tastaturfokus liegt per `:focus-within` auf dem Eltern-Container
- Icon sichtbar, wenn: `_isOpen = true` (Dropdown geöffnet) → `context-menu-container` erhält CSS-Klasse `menu-open`

**Verhalten:**
- Hover oder Fokus auf Eltern-Container: `opacity: 1` via CSS-Selektor.
- Dropdown geöffnet: `opacity: 1` via `.context-menu-container.menu-open .context-menu-toggle`.
- Fokus verlässt Container mit geöffnetem Menü: Icon bleibt sichtbar, weil `menu-open`-Klasse den Zustand hält.

**Umsetzung:** `ApplicationContextMenu`, `ApplicationGroupContextMenu` — conditional CSS-Klasse im Markup; `app.css` — CSS-Regeln.

---

## Drag & Drop — Enter-Counter für robuste Hervorhebung

**Beschreibung:** Das `ondragleave`-Event wird auch ausgelöst, wenn der Cursor von einem Eltern-Element auf ein Kind-Element wechselt, obwohl der Benutzer den Container nicht verlassen hat. Ohne Gegenmaßnahme würde die `drag-over`-Hervorhebung flackern oder frühzeitig verschwinden.

**Bedingungen:**
- `ondragenter` wird beim Eintreten in einen Container und jeden seiner Kindknoten ausgelöst.
- `ondragleave` wird beim Verlassen eines jeden Knotens ausgelöst — auch beim Wechsel zwischen Kindknoten desselben Containers.

**Verhalten:**
- `OnDragEnter(groupId)`: Wenn `groupId` neu ist, `_dragEnterCount = 1`; sonst `_dragEnterCount++`.
- `OnDragLeave()`: `_dragEnterCount--`; wenn `<= 0` → `_dropTargetGroupId = null`.
- Die `drag-over`-Klasse verschwindet erst, wenn der Counter auf 0 fällt, also wenn der Container wirklich verlassen wurde.
- Für „Ohne Gruppe" gilt das analoge Muster mit `_dragEnterCountUngrouped` und `_dropTargetIsUngrouped`.

**Umsetzung:** `ApplicationGroupTree.OnDragEnter`, `ApplicationGroupTree.OnDragLeave`, `ApplicationGroupTree.OnDragEnterUngrouped`, `ApplicationGroupTree.OnDragLeaveUngrouped`.

---

## Menüoption „Aus Gruppe entfernen" — kontextabhängige Sichtbarkeit

**Bedingung:** `Application.ApplicationGroupId.HasValue == true`

**Verhalten:** Die Option wird nur gerendert, wenn die Anwendung einer Gruppe angehört.

**Umsetzung:** `ApplicationContextMenu` — bedingte Darstellung via `@if (Application.ApplicationGroupId.HasValue)`.

---

## Systemeinträge: Schutz vor Änderung und Löschung

**Beschreibung:** Gruppe und Anwendung mit `IsSystem == true` werden ausschließlich vom `SystemEntryInitializer` verwaltet. Benutzer dürfen sie weder umbenennen/bearbeiten noch löschen.

**Bedingungen:** `ApplicationGroup.IsSystem == true` bzw. `Application.IsSystem == true`

**Verhalten:**
- REST-API: `ApplicationGroupsController.DeleteAsync` und `UpdateAsync` sowie `ApplicationsController.DeleteAsync` und `UpdateAsync` geben `403 Forbidden` zurück, wenn `IsSystem == true`.
- UI (Kontextmenü): Schaltflächen „Umbenennen" und „Löschen" in `ApplicationGroupContextMenu` sind deaktiviert (`disabled="@(Group.IsSystem)"`); Schaltflächen „Bearbeiten" und „Löschen" in `ApplicationContextMenu` sind deaktiviert (`disabled="@(Application.IsSystem)"`).
- UI (Drag & Drop): `ApplicationGroupTree.OnDragStart` bricht ab, ohne `_draggedApplication` zu setzen; `draggable` ist im Razor-Template auf `"false"` gesetzt.

**Umsetzung:** `ApplicationGroupsController.DeleteAsync`, `ApplicationGroupsController.UpdateAsync`, `ApplicationsController.DeleteAsync`, `ApplicationsController.UpdateAsync`, `ApplicationGroupContextMenu`, `ApplicationContextMenu`, `ApplicationGroupTree.OnDragStart`.

---

## Systemeintrag: automatisches Anlegen und URL-Aktualisierung

**Beschreibung:** Beim Programmstart stellt `SystemEntryInitializer` sicher, dass genau eine Systemgruppe und eine Systemanwendung vorhanden sind. Ist die URL der Systemanwendung veraltet, wird sie auf den aktuellen Wert aus `Api:BaseUrl` aktualisiert.

**Bedingungen:**
- `Api:BaseUrl` ist in `appsettings.json` konfiguriert und nicht leer.
- Es existiert keine `ApplicationGroup` mit `IsSystem == true` (Neuanlage) oder die URL der zugehörigen `Application` weicht vom konfigurierten Wert ab (Aktualisierung).

**Verhalten:**
- Gruppe fehlt: Anlegen mit `Name = "Schnittstellenzentrale"`, `IsSystem = true`.
- Anwendung fehlt: Anlegen mit `Name = "Schnittstellenzentrale"`, `IsSystem = true`, `BaseUrl = {Api:BaseUrl}`, `InterfaceUrl = {Api:BaseUrl}/swagger/v1/swagger.json`.
- URL weicht ab: `BaseUrl` und `InterfaceUrl` werden aktualisiert.
- Fehlt `Api:BaseUrl`: Warnung wird geloggt; kein Eintrag wird angelegt; Programmstart läuft weiter.
- Datenbankfehler: Exception wird abgefangen und geloggt; Programmstart wird nicht unterbrochen.
- Wiederholter Aufruf ist idempotent: Es entsteht kein Duplikat.

**Umsetzung:** `SystemEntryInitializer.InitializeAsync` — liest `Api:BaseUrl` aus `IConfiguration`; arbeitet direkt auf `IApplicationRepository` ohne HTTP-Loopback.

---

## Moduswechsel: vollständiger State-Reset

**Beschreibung:** Beim Wechsel des `StorageMode` muss der gesamte Seitenstate zurückgesetzt werden, damit keine Daten eines alten Modus sichtbar bleiben.

**Verhalten:**
- Alle offenen Dialoge und Editoren werden geschlossen.
- Der Baum wird mit den Daten des neuen Modus neu geladen.
- `OnSelectionCleared` wird ausgelöst, damit `Home` die Selektion zurücksetzt.

**Umsetzung:** `ApplicationGroupTree.OnModeChanged` — läuft via `InvokeAsync` auf dem Blazor-Synchronisierungskontext, da das Event außerhalb des Rendering-Zyklus ausgelöst werden kann.
