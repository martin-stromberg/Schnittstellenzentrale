# Enums

## `StorageMode`
Datei: `src/Schnittstellenzentrale.Core/Enums/StorageMode.cs`

| Wert | Bedeutung |
|------|-----------|
| `Team` | Team-Modus; Umgebungen sind benutzerneutral; `localStorage`-Schlüssel: `selectedEnvironmentId_Team` |
| `User` | Benutzermodus; Umgebungen sind einem konkreten Benutzer (`Owner`) zugeordnet; `localStorage`-Schlüssel: `selectedEnvironmentId_User` |

Verwendet in: `LocalStorageKeys.SelectedEnvironmentId(mode)`, `SystemEnvironment.Mode`, `IStorageModeService.CurrentMode`, `StorageModeService.SetMode`, `SystemEnvironmentRepository.ApplyOwnerFilter`
