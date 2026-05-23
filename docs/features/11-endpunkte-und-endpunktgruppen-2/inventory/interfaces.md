# Interfaces

## `ISwaggerImportService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/ISwaggerImportService.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `ImportAsync` | `Application application` | `Task<ImportDiff>` | Lädt die Swagger-Definition der Anwendung und berechnet den Diff zu den in der Datenbank gespeicherten Endpunkten |
| `ApplyDiffAsync` | `ImportDiff diff` | `Task` | Wendet alle drei Diff-Kategorien auf die Datenbank an (inkl. `ChangedEndpoints`) |

## `IApplicationRepository`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IApplicationRepository.cs`

Für den `SystemEndpointSyncService` relevante Methoden:

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `GetSystemGroupAsync` | — | `Task<ApplicationGroup?>` | Liefert die Systemgruppe mit ihren `Applications`; gibt `null` zurück, wenn keine Systemgruppe existiert |

## `IEndpointRepository`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IEndpointRepository.cs`

Für den `SystemEndpointSyncService` relevante Methoden:

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `AddEndpointAsync` | `Endpoint endpoint` | `Task<Endpoint>` | Legt einen neuen Endpunkt in der Datenbank an |
| `DeleteEndpointAsync` | `int id` | `Task` | Löscht einen Endpunkt anhand seiner ID aus der Datenbank |
