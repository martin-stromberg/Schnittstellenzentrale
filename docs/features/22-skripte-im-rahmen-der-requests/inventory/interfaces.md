# Interfaces

## `IEndpointExecutionService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IEndpointExecutionService.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `ExecuteAsync` | `Endpoint endpoint` | `Task<EndpointExecutionResult>` | Führt den HTTP-Request für den Endpunkt aus |

Das Interface hat **keine** Abhängigkeit zu `IEndpointScriptRunner` und wird bei der Implementierung der Skriptausführung **nicht** angepasst (laut Anforderung ändert sich nur die Implementierung `EndpointExecutionService`, nicht der Contract).

---

## `IActiveEnvironmentService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IActiveEnvironmentService.cs`

| Methode / Eigenschaft | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `ActiveEnvironment` | — | `SystemEnvironment?` | Aktuell aktive Umgebung |
| `ActiveVariables` | — | `IReadOnlyDictionary<string, string>` | Materialisierte Variablen der aktiven Umgebung |
| `OnActiveEnvironmentChanged` | — | `event Action?` | Wird ausgelöst, wenn die aktive Umgebung wechselt |
| `SetActiveEnvironment` | `SystemEnvironment? environment` | `void` | Setzt die aktive Umgebung und aktualisiert `ActiveVariables` |

Das Interface bietet **keinen** granularen Zugriff per Variablenname (`get(name)`, `set(name, value)`). Für das `sz.environment`-API müssen diese Operationen in `EndpointScriptRunner` über `ActiveVariables` und `SetActiveEnvironment` abgebildet werden.

---

## `IEndpointRepository`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IEndpointRepository.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `GetEndpointsAsync` | `int applicationId` | `Task<IList<Endpoint>>` | Alle Endpunkte einer Anwendung |
| `GetEndpointByIdAsync` | `int id` | `Task<Endpoint?>` | Einzelnen Endpunkt nach ID |
| `AddEndpointAsync` | `Endpoint` | `Task<Endpoint>` | Neuen Endpunkt anlegen |
| `UpdateEndpointAsync` | `Endpoint` | `Task<Endpoint>` | Endpunkt aktualisieren |
| `DeleteEndpointAsync` | `int id` | `Task` | Endpunkt löschen |
| `GetEndpointGroupsAsync` | `int applicationId` | `Task<IList<EndpointGroup>>` | Endpunktgruppen einer Anwendung |
| `GetEndpointGroupByIdAsync` | `int id` | `Task<EndpointGroup?>` | Einzelne Endpunktgruppe |
| `AddEndpointGroupAsync` | `EndpointGroup` | `Task<EndpointGroup>` | Neue Gruppe anlegen |
| `UpdateEndpointGroupAsync` | `EndpointGroup` | `Task<EndpointGroup>` | Gruppe aktualisieren |
| `DeleteEndpointGroupAsync` | `int id` | `Task` | Gruppe löschen |
| `AddHeaderAsync` | `EndpointHeader` | `Task<EndpointHeader>` | Header anlegen |
| `DeleteHeaderAsync` | `int id` | `Task` | Header löschen |
| `AddQueryParameterAsync` | `EndpointQueryParameter` | `Task<EndpointQueryParameter>` | Query-Parameter anlegen |
| `DeleteQueryParameterAsync` | `int id` | `Task` | Query-Parameter löschen |

Für `sz.execute()` (Endpunkt-Lookup nach Name) wird `GetEndpointsAsync` oder eine neue Methode `GetEndpointByNameAsync` benötigt — letztere existiert noch nicht.

---

## Noch nicht vorhandene Interfaces

Die folgenden Interfaces sind in der Anforderung neu beschrieben und existieren im Code noch nicht:

- `IEndpointScriptRunner` — Contract für die JavaScript-Skriptausführung mit `ExecuteAsync(string script, ScriptContext context)`
