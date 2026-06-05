# Enums

## `StorageMode`
Datei: `src/Schnittstellenzentrale.Core/Enums/StorageMode.cs`

| Wert | Bedeutung |
|------|-----------|
| `Team` | Team-Modus – geteilte Konfiguration (Standardwert in `StorageModeService`) |
| `User` | Benutzer-Modus – benutzerspezifische Konfiguration |

---

## `LocalStorageKeys` (statische Helferklasse)
Datei: `src/Schnittstellenzentrale.Core/Helpers/LocalStorageKeys.cs`

Enthält derzeit folgende Konstanten und Methoden:

| Member | Typ | Wert |
|--------|-----|------|
| `SelectedEnvironmentId(StorageMode mode)` | `static string` | `"selectedEnvironmentId_{mode}"` |
| `ActivityLogDisplayMode` | `const string` | `"activityLogDisplayMode"` |
| `ActivityLogPanelHeight` | `const string` | `"activityLogPanelHeight"` |

Ein Eintrag `StorageMode` (z. B. `public const string StorageMode = "storageMode"`) existiert noch **nicht**.
