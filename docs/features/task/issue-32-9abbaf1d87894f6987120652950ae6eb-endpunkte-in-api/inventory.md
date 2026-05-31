# Bestandsaufnahme: Endpunkte in API

Analysiert wurden alle für die Anforderung relevanten Artefakte: bestehende Domänenmodelle, Interfaces, Implementierungen, Blazor-Komponenten, Controller und Tests – bezogen auf die Umstellung der Endpoint/EndpointGroup-Zugriffe auf `IApplicationApiClient` und die Bereitstellung neuer REST-Endpunkte.

## Zusammenfassung

- `Endpoint` und `EndpointGroup` sind vollständig als Domänenmodelle vorhanden, inklusive `RowVersion`, `AuthenticationType`, `PreRequestScript`, `PostRequestScript`, `Headers` und `QueryParameters`.
- `IEndpointRepository` ist vollständig definiert mit Methoden für CRUD auf `Endpoint`, `EndpointGroup`, `EndpointHeader` und `EndpointQueryParameter`.
- `EndpointRepository` ist eine vollständige EF-Core-Implementierung von `IEndpointRepository`.
- `IApplicationApiClient` enthält **noch keine** Methoden für `EndpointGroup` oder `Endpoint`. Alle aktuellen Methoden beziehen sich auf `ApplicationGroup` und `Application`.
- `ApplicationApiClient` implementiert die aktuellen `IApplicationApiClient`-Methoden mit den Hilfsmethoden `BuildGetRequest`, `BuildRequestWithBody`, `BuildDeleteRequest`, `SendWithTokenAsync`, `SendWithTokenNullableAsync`, `SendWithTokenNoContentAsync` – diese sind direkt wiederverwendbar.
- Die Contracts `EndpointGroupResponse`, `EndpointResponse`, `CreateEndpointGroupRequest`, `UpdateEndpointGroupRequest`, `CreateEndpointRequest`, `UpdateEndpointRequest` **existieren noch nicht**.
- Die Controller `EndpointGroupsController` und `EndpointsController` **existieren noch nicht**. Vorhandene Muster: `ApplicationGroupsController`, `ApplicationsController`, `ApiControllerBase`.
- `ApplicationGroupTree.razor` injiziert sowohl `IApplicationApiClient` als auch `IEndpointRepository`. `ReloadApplicationDataAsync` ruft `EndpointRepository.GetEndpointGroupsAsync` und `EndpointRepository.GetEndpointsAsync` direkt auf.
- `FolderContentView.razor` injiziert nur `IEndpointRepository` und ruft `EndpointRepository.GetEndpointsAsync` in `OnParametersSetAsync` auf.
- `ApplicationContentView.razor` injiziert `IEndpointRepository` (neben anderen) und ruft `EndpointRepository.GetEndpointsAsync` in `OnParametersSetAsync` auf.
- `EndpointPage.razor` injiziert `IEndpointRepository` und verwendet es in `ForceSaveAsync` (Lesen von `RowVersion`), `SendRequestAsync` (Reload) und `PersistAsync` (Add/Update).
- `SystemEndpointSyncService` verwendet `IEndpointRepository` intern – vollständige Implementierung vorhanden.
- `SystemEntryInitializer` legt nur Gruppe und Anwendung an – **keine Endpunkte**.
- `ApplicationApiClientTests` deckt alle aktuellen Methoden (Group/Application CRUD + Get) mit gemocktem `HttpMessageHandler` ab. Tests für Endpoint-Methoden **fehlen vollständig**.
- `ApplicationApiClientIntegrationTests` **existiert noch nicht**.
- `SystemEndpointSyncServiceTests` enthält Tests für Add/Delete/Idempotenz/Fehlerbehandlung/Gruppenanlage/Auth-Erkennung/Bearer-Token. Tests für Pre-/Post-Request-Skripte und vollständige Credential-Prüfungen fehlen noch teilweise.
- `SystemEntryInitializerTests` prüft nur Gruppe- und Anwendungsanlage – **keine Endpunktprüfung**.
- `ControllerTestFactory` entfernt alle `IHostedService`-Registrierungen und stellt SQLite In-Memory-Datenbank bereit.
- `CLAUDE.md` enthält noch keinen Architekturprinzip-Abschnitt und keinen Testanforderungsabschnitt für API-Clients.

## Details

- [Datenmodell](inventory/models.md)
- [Logik](inventory/logic.md)
- [Interfaces](inventory/interfaces.md)
- [Blazor-Komponenten](inventory/components.md)
- [Tests](inventory/tests.md)
