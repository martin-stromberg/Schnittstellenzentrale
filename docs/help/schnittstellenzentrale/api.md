# Schnittstellenzentrale — API

## Übersicht

Die Schnittstellenzentrale exponiert keine REST-API. Die einzige öffentliche Schnittstelle ist der SignalR-Hub `EndpointHub`, über den Clients Echtzeit-Benachrichtigungen im Team-Modus empfangen können. Zusätzlich sind die internen C#-Interfaces dokumentiert, da sie die Erweiterbarkeit der Anwendung definieren.

---

## SignalR-Hub: `EndpointHub`

**URL:** `/hubs/endpoint`

### Client-zu-Server-Methoden

#### `SubscribeToApplication`

Abonniert Änderungsbenachrichtigungen für eine bestimmte Anwendung.

**Parameter:**

| Name | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| `applicationId` | `int` | Ja | ID der Anwendung |

#### `UnsubscribeFromApplication`

Beendet das Abonnement für eine Anwendung.

**Parameter:**

| Name | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| `applicationId` | `int` | Ja | ID der Anwendung |

#### `SubscribeToGroup`

Abonniert Änderungsbenachrichtigungen für eine Anwendungsgruppe.

**Parameter:**

| Name | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| `groupId` | `int` | Ja | ID der Gruppe |

#### `UnsubscribeFromGroup`

Beendet das Abonnement für eine Gruppe.

**Parameter:**

| Name | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| `groupId` | `int` | Ja | ID der Gruppe |

### Server-zu-Client-Events

#### `ApplicationChanged`

Wird gesendet, wenn sich eine Anwendung im Team-Modus geändert hat.

| Parameter | Typ | Beschreibung |
|-----------|-----|--------------|
| `applicationId` | `int` | ID der geänderten Anwendung |

#### `GroupChanged`

Wird gesendet, wenn sich eine Gruppe im Team-Modus geändert hat.

| Parameter | Typ | Beschreibung |
|-----------|-----|--------------|
| `groupId` | `int` | ID der geänderten Gruppe |

---

## C#-Interfaces (Erweiterungspunkte)

### `IApplicationRepository`

CRUD-Operationen für Anwendungen und Anwendungsgruppen mit StorageMode-Bewusstsein.

| Methode | Rückgabe | Beschreibung |
|---------|----------|--------------|
| `GetGroupsAsync(StorageMode, string owner)` | `Task<IList<ApplicationGroup>>` | Alle Gruppen; bei `User`-Modus nur Gruppen mit mindestens einer Anwendung des Owners |
| `GetUngroupedApplicationsAsync(StorageMode, string owner)` | `Task<IList<Application>>` | Anwendungen ohne Gruppe |
| `GetApplicationByIdAsync(int id)` | `Task<Application?>` | Anwendung inkl. Endpoints, Header, Query-Parameter, EndpointGroups |
| `AddApplicationAsync(Application)` | `Task<Application>` | Neue Anwendung anlegen |
| `UpdateApplicationAsync(Application)` | `Task<Application>` | Anwendung aktualisieren |
| `DeleteApplicationAsync(int id)` | `Task` | Anwendung löschen (Cascade auf Endpoints) |

### `IEndpointRepository`

| Methode | Rückgabe | Beschreibung |
|---------|----------|--------------|
| `GetEndpointsAsync(int applicationId)` | `Task<IList<Endpoint>>` | Alle Endpunkte einer Anwendung inkl. Header, Query-Parameter, Gruppe |
| `GetEndpointByIdAsync(int id)` | `Task<Endpoint?>` | Einzelner Endpunkt inkl. Anwendung, Header, Query-Parameter, Gruppe |
| `AddEndpointAsync(Endpoint)` | `Task<Endpoint>` | Endpunkt anlegen |
| `UpdateEndpointAsync(Endpoint)` | `Task<Endpoint>` | Endpunkt vollständig aktualisieren |
| `UpdateEndpointNameAsync(int id, string name)` | `Task` | Ausschließlich den `Name` eines Endpunkts aktualisieren; alle anderen Felder bleiben unverändert |
| `DeleteEndpointAsync(int id)` | `Task` | Endpunkt löschen (Cascade auf Header und Query-Parameter) |
| `AddHeaderAsync(EndpointHeader)` | `Task<EndpointHeader>` | Header hinzufügen |
| `DeleteHeaderAsync(int id)` | `Task` | Header entfernen |
| `AddQueryParameterAsync(EndpointQueryParameter)` | `Task<EndpointQueryParameter>` | Query-Parameter hinzufügen |
| `DeleteQueryParameterAsync(int id)` | `Task` | Query-Parameter entfernen |

### `IEndpointExecutionService`

| Methode | Rückgabe | Beschreibung |
|---------|----------|--------------|
| `ExecuteAsync(Endpoint endpoint)` | `Task<EndpointExecutionResult>` | Führt den Endpunkt aus und gibt Ergebnis zurück |

### `IHealthCheckService`

| Methode | Rückgabe | Beschreibung |
|---------|----------|--------------|
| `CheckAsync(Application application)` | `Task<bool?>` | `true` = erreichbar, `false` = nicht erreichbar, `null` = Cooldown aktiv |

### `ISwaggerImportService` / `IODataImportService`

| Methode | Rückgabe | Beschreibung |
|---------|----------|--------------|
| `ImportAsync(Application application)` | `Task<ImportDiff>` | Ruft Definition ab, berechnet Diff gegen bestehende Endpunkte |

### `ICredentialService`

| Methode | Rückgabe | Beschreibung |
|---------|----------|--------------|
| `GetPassword(string target)` | `string?` | Liest Passwort/Token aus Windows Credential Manager |
| `SavePassword(string target, string username, string password)` | `void` | Speichert Passwort/Token im Windows Credential Manager |
| `DeletePassword(string target)` | `void` | Löscht Eintrag aus Windows Credential Manager |

**Schlüsselformat für `target`:** `Schnittstellenzentrale:{ApplicationId}:{AuthenticationType}`

### `ISignalRNotificationService`

| Methode | Rückgabe | Beschreibung |
|---------|----------|--------------|
| `NotifyApplicationChangedAsync(int applicationId)` | `Task` | Sendet `ApplicationChanged` an Gruppe `application:{id}` |
| `NotifyGroupChangedAsync(int groupId)` | `Task` | Sendet `GroupChanged` an Gruppe `group:{id}` |
