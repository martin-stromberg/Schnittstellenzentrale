# Speichermodus — API

## Übersicht

Der Speichermodus ist über das `IStorageModeService`-Interface zugänglich. Blazor-Komponenten binden den Service per Dependency Injection ein und können den aktuellen Modus lesen sowie ändern.

## Interface `IStorageModeService`

Namespace: `Schnittstellenzentrale.Core.Interfaces`

### `CurrentMode`

**Beschreibung:** Gibt den aktuell aktiven Speichermodus zurück.

| Eigenschaft | Typ | Beschreibung |
|-------------|-----|--------------|
| `CurrentMode` | `StorageMode` | Aktuell aktiver Modus; Standardwert: `StorageMode.Team` |

### `OnModeChanged`

**Beschreibung:** Wird ausgelöst, wenn der Speichermodus geändert wurde. Komponenten abonnieren dieses Event, um sich nach einem Moduswechsel neu zu rendern.

| Member | Typ | Beschreibung |
|--------|-----|--------------|
| `OnModeChanged` | `event Action?` | Wird nach jedem erfolgreichen Aufruf von `SetMode` mit einem neuen Wert ausgelöst. |

### `InitializeAsync()`

**Beschreibung:** Liest den gespeicherten Modus aus `localStorage` und setzt `CurrentMode` entsprechend. Muss einmalig nach dem ersten Render von `AppShell` aufgerufen werden.

**Parameter:** keine

**Rückgabe:** `Task`

**Hinweis:** `AppShell` ruft diese Methode in `OnAfterRenderAsync(firstRender: true)` auf, sequenziell vor `RestoreEnvironmentFromLocalStorageAsync`. Andere Komponenten müssen sie nicht erneut aufrufen.

### `SetMode(StorageMode mode)`

**Beschreibung:** Wechselt den aktiven Speichermodus, persistiert die Auswahl fire-and-forget in `localStorage` und löst `OnModeChanged` aus.

**Parameter:**

| Name | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| `mode` | `StorageMode` | Ja | Der neue Speichermodus (`StorageMode.Team` oder `StorageMode.User`). |

**Rückgabe:** `void`

**Verhalten:** Wenn `mode` mit dem aktuellen `CurrentMode` identisch ist, wird weder `localStorage` geschrieben noch `OnModeChanged` ausgelöst.

**Beispiel:**
```csharp
@inject IStorageModeService StorageModeService

StorageModeService.SetMode(StorageMode.User);
```

## Enum `StorageMode`

Namespace: `Schnittstellenzentrale.Core.Enums`

| Wert | Beschreibung |
|------|--------------|
| `Team` | Geteilter Teamdatensatz (Standard) |
| `User` | Benutzerspezifischer Datensatz |

## Klasse `LocalStorageKeys`

Namespace: `Schnittstellenzentrale.Core.Helpers`

| Konstante / Methode | Typ | Wert / Format | Beschreibung |
|---------------------|-----|---------------|--------------|
| `StorageMode` | `const string` | `"storageMode"` | `localStorage`-Schlüssel für den gespeicherten Modus |
| `SelectedEnvironmentId(StorageMode mode)` | `static string` | `"selectedEnvironmentId_Team"` / `"selectedEnvironmentId_User"` | `localStorage`-Schlüssel für die zuletzt gewählte Umgebung je Modus |

## JavaScript-Modul `storage-mode.js`

Das Modul wird ausschliesslich von `StorageModeService` via JS-Interop aufgerufen. Es ist kein öffentliches API, kann aber bei Bedarf direkt in anderen JS-Modulen importiert werden.

| Funktion | Parameter | Rückgabe | Beschreibung |
|----------|-----------|----------|--------------|
| `getStoredMode()` | — | `string \| null` | Liest den gespeicherten Modusstring aus `localStorage` (Schlüssel: `storageMode`). |
| `setStoredMode(value)` | `value: string` | `void` | Schreibt den Modusstring in `localStorage`. |
