# Interfaces – Bestandsaufnahme

## `IStorageModeService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IStorageModeService.cs`

Direktes strukturelles Vorbild für das zu erstellende `IThemeService`-Interface. Zeigt das etablierte Muster: eine lesbare Eigenschaft für den aktuellen Wert, ein `Action`-Event für Änderungen und eine Setter-Methode.

| Member | Typ | Zweck |
|--------|-----|-------|
| `CurrentMode` | `StorageMode` (get) | Liefert den aktiven Speichermodus. |
| `OnModeChanged` | `event Action?` | Wird bei jedem Moduswechsel ausgelöst. |
| `SetMode(StorageMode mode)` | `void` | Setzt den neuen Modus. |

---

> **Hinweis:** Ein `IThemeService`-Interface existiert noch nicht im Projekt.
