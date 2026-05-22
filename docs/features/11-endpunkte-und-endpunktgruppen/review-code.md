# Code-Review

## Ergebnis

**Status:** Erledigt

## Befunde

### EndpointExecutionService.cs / RequestAuthPanel.razor

- ~~**Doppelter Code** — `BuildCredentialTarget` in `EndpointExecutionService` (Zeile 180–181) erzeugt denselben Format-String `"Schnittstellenzentrale:{applicationId}:{authenticationType}"` wie `BuildCredentialTarget` in `RequestAuthPanel.razor` (Zeile 78–79). Die Schlüssel-Bildungslogik ist an zwei voneinander unabhängigen Stellen dupliziert.~~

  **Erledigt:** `CredentialTargetHelper.Build(int applicationId, AuthenticationType authenticationType)` in `Schnittstellenzentrale.Core/Helpers/CredentialTargetHelper.cs` angelegt. Beide Stellen rufen den Helper auf; lokale Methoden entfernt.

### EndpointExecutionService.cs (EndpointExecutionService)

- ~~**Fehlerbehandlung** — Der `default`-Fall im `switch`-Ausdruck in `BuildRequest` (Zeile 138: `_ => System.Net.Http.HttpMethod.Get`) fällt bei unbekannten `HttpMethod`-Enum-Werten stillschweigend auf GET zurück. Neue Enum-Werte werden dadurch nicht entdeckt.~~

  **Erledigt:** `default`-Fall wirft jetzt `ArgumentOutOfRangeException`.

- ~~**Toter Code** — In `BuildResult` (Zeile 115) ist der Null-Check `body == null ? 0 :` toter Code, da `ReadAsStringAsync()` nie `null` zurückgibt.~~

  **Erledigt:** Ausdruck vereinfacht auf `Encoding.UTF8.GetByteCount(body)`.

### ApplicationGroupTree.razor (ApplicationGroupTree)

- ~~**Toter Code** — `GetUngroupedEndpointsAsync` aus `IEndpointRepository` wird im gesamten Branch nie aufgerufen. `ReloadApplicationDataAsync` (Zeile 171–181) verwendet stets `GetEndpointsAsync` und filtert ungrouped Endpunkte per `.Where(e => e.EndpointGroupId == null)` im Template.~~

  **Erledigt:** Methode aus `IEndpointRepository` und `EndpointRepository` entfernt.

- ~~**Fehlerbehandlung** — Der `catch`-Block in `ConnectHubAsync` (Zeile 141–144) schluckt die Ausnahme still. SignalR-Verbindungsprobleme sind damit nicht diagnostizierbar.~~

  **Erledigt:** `ILogger<ApplicationGroupTree>` injiziert; Ausnahme wird auf Debug-Level protokolliert.

### Home.razor (Home)

- ~~**Fehlende Kapselung / Doppelter Code** — In `HandleDeleteEndpointGroupRequested` (Zeile 341) wird `GetEndpointsAsync` aufgerufen, um die Anzahl der Endpunkte in der Gruppe zu zählen. In `OnEndpointGroupDeleteConfirmed` (Zeile 357) wird `GetEndpointsAsync` für dieselbe `applicationId` erneut aufgerufen, um den ausgewählten Endpunkt zu prüfen.~~

  **Erledigt:** Snapshot in neuem Feld `_deleteTargetEndpoints` gespeichert; `OnEndpointGroupDeleteConfirmed` lädt nicht mehr neu.

- ~~**Fehlende Kapselung** — Das Muster `CloseAllPanels(); _selectedApplicationId = null; _selectedEndpoint = null;` tritt in `OnCreateGroupRequested` (Zeile 131–135), `OnCreateApplicationRequested` (Zeile 139–143) und `HandleEndpointSelected` (Zeile 253–256) in leicht abweichender Reihenfolge auf.~~

  **Erledigt:** Alle drei Stellen rufen jetzt `ClearSelection()` auf (bereits vorhandene Methode, die exakt dieses Muster kapselt).

### EndpointPage.razor (EndpointPage)

- ~~**Fehlende Kapselung** — `OnParametersSetAsync` (Zeile 146) prüft `Endpoint.Id != _lastLoadedEndpointId`. Das ist korrekt und ausreichend. Es gibt jedoch keinen Schutz davor, dass `LoadModelFromParameter` bei künftiger Erweiterung des `Endpoint`-Modells vergisst, neue Felder zu kopieren.~~

  **Erledigt:** XML-Kommentar zu `LoadModelFromParameter` ergänzt, der explizit auf die Pflicht hinweist, alle Felder zu spiegeln.

### EndpointExecutionServiceTests.cs

- ~~**Doppelter Code** — `Execute_WithAuthTypeNegotiate_UsesNegotiateHandler` (Zeilen 91–110) und `Execute_WithAuthTypeNegotiateWithImpersonation_RunsImpersonated` (Zeilen 132–152) besitzen identisches `handlerMock`/`factoryMock`-Setup und verifizieren denselben `CreateClient("negotiate")`-Aufruf. Einziger Unterschied: der `AuthenticationType`.~~

  **Erledigt:** Zu `Execute_WithNegotiateAuthType_UsesNegotiateHandler` mit `[Theory] [InlineData(AuthenticationType.Negotiate)] [InlineData(AuthenticationType.NegotiateWithImpersonation)]` zusammengeführt.

- ~~**Doppelter Code** — `CreateService` (Zeilen 34–52) und `CreateServiceWithBody` (Zeilen 229–247) unterscheiden sich ausschließlich im Response-Body. Der gesamte Setup-Code für `handlerMock`, `factoryMock` und `EndpointExecutionService` ist identisch.~~

  **Erledigt:** `CreateService` um optionalen `body`-Parameter (`string body = "{}"`) erweitert; `CreateServiceWithBody` entfernt.

### TestHelpers.cs

- ~~**Toter Code** — `ExecuteWithTwoEndpointContextsAsync` (Zeile 56–74) wird von keinem Test aufgerufen.~~

  **Nicht zutreffend:** `SaveEndpoint_ConcurrentWrite_DetectsConflict` in `EndpointRepositoryIntegrationTests` ruft `ExecuteWithTwoEndpointContextsAsync` bereits auf. Der Befund war zum Zeitpunkt der Review-Erstellung korrekt, die Methode wurde jedoch im selben Zug genutzt.

## Geprüfte Dateien

- `src/Schnittstellenzentrale.Core/Interfaces/ISignalRNotificationService.cs`
- `src/Schnittstellenzentrale.Core/Models/Endpoint.cs`
- `src/Schnittstellenzentrale.Core/Models/EndpointExecutionResult.cs`
- `src/Schnittstellenzentrale.Core/Enums/BodyMode.cs`
- `src/Schnittstellenzentrale.Infrastructure/Data/AppDbContext.cs`
- `src/Schnittstellenzentrale.Infrastructure/Repositories/EndpointRepository.cs`
- `src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionService.cs`
- `src/Schnittstellenzentrale.Infrastructure/Services/SignalRNotificationService.cs`
- `src/Schnittstellenzentrale.Tests/Helpers/TestHelpers.cs`
- `src/Schnittstellenzentrale.Tests/Integration/EndpointRepositoryIntegrationTests.cs`
- `src/Schnittstellenzentrale.Tests/Services/EndpointExecutionServiceTests.cs`
- `src/Schnittstellenzentrale.Tests/Components/EndpointContextMenuTests.cs`
- `src/Schnittstellenzentrale.Tests/Components/EndpointGroupContextMenuTests.cs`
- `src/Schnittstellenzentrale/Components/Pages/Home.razor`
- `src/Schnittstellenzentrale/Components/Shared/ApplicationCard.razor`
- `src/Schnittstellenzentrale/Components/Shared/ApplicationContextMenu.razor`
- `src/Schnittstellenzentrale/Components/Shared/ApplicationGroupTree.razor`
- `src/Schnittstellenzentrale/Components/Shared/ConfirmDeleteEndpointGroupDialog.razor`
- `src/Schnittstellenzentrale/Components/Shared/EndpointContextMenu.razor`
- `src/Schnittstellenzentrale/Components/Shared/EndpointGroupContextMenu.razor`
- `src/Schnittstellenzentrale/Components/Shared/EndpointPage.razor`
- `src/Schnittstellenzentrale/Components/Shared/RenameEndpointGroupDialog.razor`
- `src/Schnittstellenzentrale/Components/Shared/RequestAuthPanel.razor`
- `src/Schnittstellenzentrale/Components/Shared/RequestBodyPanel.razor`
- `src/Schnittstellenzentrale/Components/Shared/RequestHeadersPanel.razor`
- `src/Schnittstellenzentrale/Components/Shared/RequestQueryParamsPanel.razor`
- `src/Schnittstellenzentrale/Components/Shared/ResponseBodyPanel.razor`
- `src/Schnittstellenzentrale/Components/Shared/ResponseHeadersPanel.razor`
- `src/Schnittstellenzentrale/wwwroot/endpoint-page.js`
