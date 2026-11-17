# Detail: Alle mutierenden Pfade und Benachrichtigungs-Ist-Zustand

Legende: ✅ benachrichtigt heute (Team-geprüft) · ❌ benachrichtigt nicht

## REST-API (`api/…`)

| Pfad | Entität | Benachrichtigung heute | Benachrichtigungsart (Soll) |
|------|---------|----------------------|------------------------------|
| `POST /api/applications` | Application | ✅ TreeChanged | TreeChanged |
| `PUT /api/applications/{id}` | Application | ✅ TreeChanged | TreeChanged |
| `DELETE /api/applications/{id}` | Application | ✅ TreeChanged | TreeChanged |
| `POST /api/application-groups` | ApplicationGroup | ✅ TreeChanged | TreeChanged |
| `PUT /api/application-groups/{id}` | ApplicationGroup | ✅ TreeChanged | TreeChanged |
| `DELETE /api/application-groups/{id}` | ApplicationGroup | ✅ TreeChanged | TreeChanged |
| `POST /api/endpoints` | Endpoint | ✅ EndpointChanged | EndpointChanged |
| `PUT /api/endpoints/{id}` | Endpoint | ✅ EndpointChanged | EndpointChanged |
| `DELETE /api/endpoints/{id}` | Endpoint | ✅ EndpointChanged | EndpointChanged |
| `POST /api/endpoints/headers` | Endpoint (Kind) | ❌ | EndpointChanged |
| `DELETE /api/endpoints/headers/{id}` | Endpoint (Kind) | ❌ | EndpointChanged |
| `POST /api/endpoints/query-parameters` | Endpoint (Kind) | ❌ | EndpointChanged |
| `DELETE /api/endpoints/query-parameters/{id}` | Endpoint (Kind) | ❌ | EndpointChanged |
| `POST /api/applications/{id}/odata-import/apply` | Endpoint/Group (mehrere) | ❌ | TreeChanged (eine Nachricht) |
| `POST /api/applications/{id}/swagger-import/apply` | Endpoint/Group (mehrere) | ❌ | TreeChanged (eine Nachricht) |
| `GET`-Endpunkte (alle) | – | nicht mutierend | – |

## OData-API (`odatav4/…`) — heute überall ❌

| Pfad | Entity-Set | Benachrichtigungsart (Soll) |
|------|-----------|------------------------------|
| `POST /odatav4/Applications` | Applications | TreeChanged |
| `PUT /odatav4/Applications({key})` | Applications | TreeChanged |
| `PATCH /odatav4/Applications({key})` | Applications | TreeChanged |
| `DELETE /odatav4/Applications({key})` | Applications | TreeChanged |
| `POST /odatav4/ApplicationGroups` | ApplicationGroups | TreeChanged |
| `PUT /odatav4/ApplicationGroups({key})` | ApplicationGroups | TreeChanged |
| `PATCH /odatav4/ApplicationGroups({key})` | ApplicationGroups | TreeChanged |
| `DELETE /odatav4/ApplicationGroups({key})` | ApplicationGroups | TreeChanged |
| `POST /odatav4/Endpoints` | Endpoints | EndpointChanged |
| `PUT /odatav4/Endpoints({key})` | Endpoints | EndpointChanged |
| `PATCH /odatav4/Endpoints({key})` | Endpoints | EndpointChanged |
| `DELETE /odatav4/Endpoints({key})` | Endpoints | EndpointChanged |
| `POST /odatav4/EndpointGroups` | EndpointGroups | EndpointGroupChanged |
| `PUT /odatav4/EndpointGroups({key})` | EndpointGroups | EndpointGroupChanged |
| `PATCH /odatav4/EndpointGroups({key})` | EndpointGroups | EndpointGroupChanged |
| `DELETE /odatav4/EndpointGroups({key})` | EndpointGroups | EndpointGroupChanged |

## Nicht-API-Pfade

| Pfad | Beschreibung | Soll |
|------|--------------|------|
| `SystemEndpointSyncService` | Startabgleich über `SyncFromDocumentAsync` | genau eine TreeChanged bei tatsächlichen Änderungen |
| `EndpointPage.razor OnSaveSuccessAsync` | UI-Speichern (läuft über REST-Controller) | eigene Benachrichtigung entfernen (Controller benachrichtigt) |
| `EnvironmentManagementOverlay`, `EndpointScriptRunner` | Umgebungen | unverändert (außerhalb Scope) |

## Hinweise zur Parameterermittlung

- REST `DELETE /api/endpoints/headers/{id}` und `DELETE …/query-parameters/{id}` liefern nur die Kind-ID; die `applicationId` muss über `header.EndpointId`/`parameter.EndpointId` → `GetEndpointByIdAsync` aufgelöst werden.
- OData `Endpoints`-Mutationen: `applicationId` liegt auf der Entität (`existing.ApplicationId` bzw. `saved.ApplicationId`).
- OData `EndpointGroups`-Mutationen: `applicationId` liegt auf der Entität (`entity.ApplicationId`/`existing.ApplicationId`).
- Import-Apply: `ImportDiff` besitzt `NewEndpoints`/`ChangedEndpoints`/`RemovedEndpoints`; Benachrichtigung nur, wenn mindestens eine Liste nicht leer ist und kein `ErrorMessage` vorliegt.
