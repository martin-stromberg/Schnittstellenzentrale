# Verwaltung der Anwendungen — Business Rules

## Owner-Zuweisung im Benutzermodus

**Beschreibung:** Im Benutzermodus (`StorageMode.User`) müssen Anwendungen einem konkreten Benutzer gehören, damit die Filterfunktion in `GetApplicationsAsync` nur die eigenen Einträge zurückgibt. Da der Anwender die `Owner`-Eigenschaft nicht selbst eingibt, setzt die Komponente diesen Wert automatisch.

**Bedingungen:**
- `IStorageModeService.CurrentMode == StorageMode.User`

**Verhalten:**
- Wenn `StorageMode.User`: `_model.Owner` wird vor dem Speichern auf `ICurrentUserService.GetCurrentUserName()` gesetzt.
- Wenn `StorageMode.Team`: `_model.Owner` bleibt `null`; die Anwendung ist für alle Benutzer sichtbar.

**Umsetzung:** `ApplicationEditor.SaveAsync` — Die Zuweisung erfolgt unmittelbar vor dem Aufruf von `AddApplicationAsync`, damit kein manuell eingegebener Wert versehentlich überschrieben werden kann.

---

## Gegenseitiger Ausschluss der Editoren

**Beschreibung:** Da beide Formulare in derselben schmalen Seitenleiste platziert sind, darf immer nur eines sichtbar sein, um Platzkonflikte zu vermeiden.

**Bedingungen:** Der Anwender klickt auf „Neue Gruppe" oder „Neue Anwendung".

**Verhalten:**
- Wenn „Neue Gruppe" angeklickt: `_showGroupEditor = true`, `_showApplicationEditor = false`.
- Wenn „Neue Anwendung" angeklickt: `_showApplicationEditor = true`, `_showGroupEditor = false`.

**Umsetzung:** `ApplicationGroupTree.ShowGroupEditor` / `ApplicationGroupTree.ShowApplicationEditor`.

---

## SignalR-Benachrichtigung nur im Teammodus

**Beschreibung:** Live-Updates via SignalR sind nur im Teammodus sinnvoll, da im Benutzermodus jeder Benutzer ausschließlich seine eigenen Daten sieht und keine anderen Sitzungen von einer Änderung betroffen sind.

**Bedingungen:**
- `IStorageModeService.CurrentMode == StorageMode.Team`

**Verhalten:**
- Wenn `StorageMode.Team`: Nach dem Speichern wird `ISignalRNotificationService.NotifyGroupChangedAsync` bzw. `NotifyApplicationChangedAsync` aufgerufen.
- Wenn `StorageMode.User`: Keine SignalR-Benachrichtigung.

**Umsetzung:** `ApplicationGroupEditor.SaveAsync` und `ApplicationEditor.SaveAsync`.
