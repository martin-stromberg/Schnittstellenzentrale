# Interfaces

## `IStorageModeService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IStorageModeService.cs`

| Methode / Eigenschaft | Parameter | Rückgabewert | Zweck |
|-----------------------|-----------|--------------|-------|
| `CurrentMode` | — | `StorageMode` | Liefert den aktuell aktiven Modus |
| `OnModeChanged` | — | `event Action?` | Wird ausgelöst, wenn der Modus wechselt |
| `SetMode` | `StorageMode mode` | `void` | Setzt den aktiven Modus |

Wird von `MainLayout` abonniert (`OnModeChanged`) und soll als Vorlage für `IActiveEnvironmentService` dienen.

---

## `ISignalRNotificationService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/ISignalRNotificationService.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `NotifyApplicationChangedAsync` | `int applicationId` | `Task` | Benachrichtigt Clients über Anwendungsänderungen |
| `NotifyGroupChangedAsync` | `int groupId` | `Task` | Benachrichtigt Clients über Gruppenänderungen |
| `NotifyEndpointChangedAsync` | `int endpointId, int applicationId` | `Task` | Benachrichtigt Clients über Endpunktänderungen |
| `NotifyEndpointGroupChangedAsync` | `int endpointGroupId, int applicationId` | `Task` | Benachrichtigt Clients über Endpunktgruppenänderungen |

Die Anforderung sieht vor, `NotifyEnvironmentChangedAsync()` hinzuzufügen, um Änderungen an `SystemEnvironment`-Einträgen im Team-Modus zu melden.

---

## `IEndpointExecutionService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IEndpointExecutionService.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `ExecuteAsync` | `Endpoint endpoint` | `Task<EndpointExecutionResult>` | Führt einen Endpunkt aus und liefert Ergebnis |

Die Implementierung `EndpointExecutionService` soll um `IActiveEnvironmentService` erweitert und eine private Methode `ResolvePlaceholders` ergänzt werden.

---

## `ICurrentUserService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/ICurrentUserService.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `GetCurrentUserName` | — | `string` | Gibt den aktuellen Windows-Benutzernamen zurück |

Relevant für das `Owner`-Feld von `SystemEnvironment` im User-Modus (offene Frage aus der Anforderung).

---

## `IApplicationRepository`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IApplicationRepository.cs`

Dient als strukturelle Vorlage für das zu erstellende `ISystemEnvironmentRepository`. Das Muster `GetXxxAsync(StorageMode, string owner)` ist hier etabliert.

---

## Fehlende Interfaces

Die Anforderung sieht folgende neue Interfaces vor, die noch nicht existieren:

- `ISystemEnvironmentRepository` — CRUD für `SystemEnvironment` mit `GetEnvironmentsAsync(StorageMode, string? owner)`, `GetByIdAsync(int id)`, `AddAsync`, `UpdateAsync`, `DeleteAsync`
- `IActiveEnvironmentService` — Hält die aktive `SystemEnvironment` und materialisierte `ActiveVariables` als `IReadOnlyDictionary<string, string>` (Scoped-Service)
