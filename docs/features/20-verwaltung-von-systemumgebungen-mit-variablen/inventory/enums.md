# Enums

## `StorageMode`
Datei: `src/Schnittstellenzentrale.Core/Enums/StorageMode.cs`

| Wert | Bedeutung |
|------|-----------|
| `Team` | Gemeinsamer Modus für alle Benutzer |
| `User` | Benutzerspezifischer Modus (gefiltert nach `Owner`) |

Wird von `IStorageModeService` / `StorageModeService` verwaltet und im `MainLayout` über eine `<select>`-Box umgeschaltet. Soll als Modus-Zuordnung (`Mode`-Eigenschaft) für `SystemEnvironment` wiederverwendet werden.

Referenzverwendungen:
- `ApplicationRepository.ApplyOwnerFilter` filtert nach `Owner`, wenn `StorageMode.User` aktiv ist — dieses Muster soll für `SystemEnvironmentRepository.GetEnvironmentsAsync` übernommen werden.
- `IStorageModeService.CurrentMode` liefert den aktuell aktiven Modus an UI-Komponenten.
