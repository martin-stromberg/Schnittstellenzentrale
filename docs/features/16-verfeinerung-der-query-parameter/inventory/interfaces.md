# Interfaces

## `IEndpointExecutionService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IEndpointExecutionService.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `ExecuteAsync` | `Endpoint endpoint` | `Task<EndpointExecutionResult>` | Führt einen Endpunkt aus und gibt das Ergebnis zurück. Wird von `EndpointPage.SendRequestAsync` aufgerufen. |

Implementiert durch: `EndpointExecutionService` (`src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionService.cs`)
