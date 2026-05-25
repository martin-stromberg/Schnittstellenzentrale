# Interfaces

## `ISwaggerImportService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/ISwaggerImportService.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `ImportAsync` | `Application application` | `Task<ImportDiff>` | Ruft die Swagger-Definition ab, parst sie und berechnet den Diff zu den bestehenden Endpunkten |
| `ApplyDiffAsync` | `ImportDiff diff` | `Task` | Persistiert den berechneten Diff (Anlegen, Ändern, Löschen von Endpunkten) |

---

## `ICredentialService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/ICredentialService.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `GetPassword` | `string target` | `string?` | Liest ein Passwort / Token aus dem Windows Credential Manager |
| `SavePassword` | `string target, string username, string password` | `void` | Speichert ein Passwort / Token im Windows Credential Manager |
| `DeletePassword` | `string target` | `void` | Löscht einen Eintrag aus dem Windows Credential Manager |

Hinweis: `ICredentialService` wird aktuell **nicht** in `SwaggerImportService` injiziert. `EndpointExecutionService` verwendet `ICredentialService` in `ApplyAuthentication`, um den Bearer-Token für die Ausführung zu lesen. Der Schlüssel für den Credential-Zugriff wird durch `CredentialTargetHelper.Build(applicationId, authenticationType)` gebildet.

---

## `IEndpointRepository`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IEndpointRepository.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `GetEndpointsAsync` | `int applicationId` | `Task<IList<Endpoint>>` | Alle Endpunkte einer Anwendung laden |
| `GetEndpointByIdAsync` | `int id` | `Task<Endpoint?>` | Einzelnen Endpunkt per Id laden |
| `GetEndpointByNameAsync` | `int applicationId, string name` | `Task<IList<Endpoint>>` | Endpunkte per Name suchen (für `sz.execute`) |
| `AddEndpointAsync` | `Endpoint endpoint` | `Task<Endpoint>` | Neuen Endpunkt persistieren |
| `UpdateEndpointAsync` | `Endpoint endpoint` | `Task<Endpoint>` | Bestehenden Endpunkt aktualisieren |
| `DeleteEndpointAsync` | `int id` | `Task` | Endpunkt löschen |
| `GetEndpointGroupsAsync` | `int applicationId` | `Task<IList<EndpointGroup>>` | Alle Gruppen einer Anwendung laden |
| `GetEndpointGroupByIdAsync` | `int id` | `Task<EndpointGroup?>` | Einzelne Gruppe laden |
| `AddEndpointGroupAsync` | `EndpointGroup group` | `Task<EndpointGroup>` | Neue Gruppe anlegen |
| `UpdateEndpointGroupAsync` | `EndpointGroup group` | `Task<EndpointGroup>` | Gruppe aktualisieren |
| `DeleteEndpointGroupAsync` | `int id` | `Task` | Gruppe löschen |
| `AddHeaderAsync` | `EndpointHeader header` | `Task<EndpointHeader>` | Header hinzufügen |
| `DeleteHeaderAsync` | `int id` | `Task` | Header löschen |
| `AddQueryParameterAsync` | `EndpointQueryParameter parameter` | `Task<EndpointQueryParameter>` | Query-Parameter hinzufügen |
| `DeleteQueryParameterAsync` | `int id` | `Task` | Query-Parameter löschen |

---

## `IEndpointScriptRunner`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IEndpointScriptRunner.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `ExecuteAsync` | `string script, ScriptContext context` | `Task<ScriptExecutionResult>` | Führt ein JavaScript-Skript im Kontext des übergebenen `ScriptContext` aus |
