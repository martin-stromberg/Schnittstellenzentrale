# Interfaces

## `IStorageModeService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IStorageModeService.cs`

| Member | Parameter | Rückgabewert | Zweck |
|--------|-----------|--------------|-------|
| `CurrentMode` | — | `StorageMode` | Liefert den aktuell gesetzten Speichermodus |
| `OnModeChanged` | — | `event Action?` | Wird nach jedem Moduswechsel ausgelöst |
| `SetMode(StorageMode mode)` | `mode: StorageMode` | `void` | Setzt den aktiven Modus und feuert `OnModeChanged` |

Eine Methode `InitializeAsync()` (analog `IThemeService.InitializeAsync()`) ist noch **nicht** vorhanden.

---

## `IThemeService` (Referenzinterface)
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IThemeService.cs`

| Member | Parameter | Rückgabewert | Zweck |
|--------|-----------|--------------|-------|
| `CurrentScheme` | — | `ColorScheme` | Liefert das aktuell gesetzte Farbschema |
| `OnThemeChanged` | — | `event Action?` | Wird nach jedem Theme-Wechsel ausgelöst |
| `InitializeAsync()` | — | `Task` | Liest das gespeicherte Theme aus `localStorage` und setzt `CurrentScheme` |
| `SetTheme(ColorScheme scheme)` | `scheme: ColorScheme` | `Task` | Setzt das Theme, persistiert es und feuert `OnThemeChanged` |

Dient als Referenzmuster für die geplante Erweiterung von `IStorageModeService`.
