# JavaScript-Bestandsaufnahme

## `endpoint-page.js`
Datei: `src/Schnittstellenzentrale/wwwroot/endpoint-page.js`

Dieses Modul ist das einzige projektspezifische JS-Modul und wird von `ApplicationGroupTree.razor` über `import('./endpoint-page.js')` geladen.

| Funktion | Zweck |
|----------|-------|
| `registerSaveShortcut(dotnetHelper)` | Registriert Ctrl+S-Shortcut für DotNet-Callback |
| `unregisterSaveShortcut()` | Entfernt den Keydown-Handler |
| `enableBeforeUnloadGuard()` | Aktiviert Warnung beim Schließen des Tabs |
| `disableBeforeUnloadGuard()` | Deaktiviert Warnung |
| `getStoredSidebarWidth()` | Liest `sidebarWidth` aus `localStorage` |
| `setStoredSidebarWidth(value)` | Schreibt `sidebarWidth` in `localStorage` |
| `initializeSidebarResize(handleElement, sidebarElement)` | Fügt `mousedown`/`pointermove`/`mouseup`-Listener für Sidebar-Resize hinzu; speichert neue Breite automatisch |
| `applyStoredSidebarWidth(sidebarElement)` | Wendet gespeicherte Sidebar-Breite an |

`localStorage`-Schlüssel: `sidebarWidth`

Hinweis: Das Modul stellt bereits ein vollständiges Muster für `initializeSidebarResize` bereit — Pointer-Events-Logik, `localStorage`-Zugriff und Auto-Save. Das geplante `activity-log-panel.js` kann dieses Muster für Panel-Resize und `localStorage`-Persistierung von Anzeigemodus und Panelhöhe übernehmen.

---

## `LocalStorageKeys`
Datei: `src/Schnittstellenzentrale.Core/Helpers/LocalStorageKeys.cs`

| Schlüssel (statische Methode / Konstante) | Rückgabewert | Zweck |
|-------------------------------------------|--------------|-------|
| `SelectedEnvironmentId(StorageMode mode)` | `"selectedEnvironmentId_{mode}"` | localStorage-Schlüssel für gespeicherte Umgebungs-ID |

Hinweis: Die Konstanten `ActivityLogDisplayMode` (`"activityLogDisplayMode"`) und `ActivityLogPanelHeight` (`"activityLogPanelHeight"`) fehlen noch.
