# Enums

## `StorageMode`
Datei: `src/Schnittstellenzentrale.Core/Enums/StorageMode.cs`

| Wert | Bedeutung |
|---|---|
| `Team` | Team-Modus: Alle Anwendungen und Gruppen sind sichtbar und werden geteilt |
| `User` | Benutzer-Modus: Nur eigene Anwendungen (gefiltert nach `Owner`) sind sichtbar |

Verwendet in `IStorageModeService.CurrentMode`, `IApplicationRepository`-Methoden und in `ApplicationRepository.ApplyOwnerFilter`. Im `StorageMode.Team` werden SignalR-Benachrichtigungen ausgelöst.
