# Code-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### EndpointExecutionService.cs (EndpointExecutionService)

- **Fehlende Kapselung / Doppelter Code** — `ExecuteWithAuthAsync` (Zeile 53–64) und `ExecuteImpersonatedAsync` (Zeile 66–77) bauen beide einen Request via `BuildRequest` auf und rufen `BuildResult` auf, jedoch dupliziert `ExecuteImpersonatedAsync` das Muster vollständig als Lambda. Sollte sich die Logik in `BuildRequest` oder `BuildResult` ändern, muss `ExecuteImpersonatedAsync` separat angepasst werden.

  Empfehlung: Eine gemeinsame Hilfsmethode `SendAndBuildResultAsync(HttpClient client, Endpoint endpoint)` extrahieren, die `BuildRequest`, den HTTP-Send und `BuildResult` zusammenfasst. Beide `Execute*`-Methoden rufen dann diese Hilfsmethode auf.

- **Fehlerbehandlung — zu breiter Exception-Handler** — Der `catch (Exception ex)`-Block in `ExecuteAsync` (Zeile 44–50) fängt alle Ausnahmen einschließlich `OperationCanceledException`. Das verschluckt Abbruchanforderungen, die vom `CancellationToken`-Mechanismus kommen sollten.

  Empfehlung: `OperationCanceledException` (bzw. `TaskCanceledException`) vor dem generellen `catch`-Block explizit behandeln oder weiterwerfen, damit Abbrüche nicht als normale Fehler erscheinen.

- **Hardcodierter Wert** — Das Credential-Ziel wird in `ApplyAuthentication` (Zeile 131) als Literal `"Schnittstellenzentrale:{endpoint.ApplicationId}:{endpoint.AuthenticationType}"` aufgebaut. Dieses Format ist entscheidend, aber nur an einer Stelle definiert; Aufrufer, die Credentials speichern, müssen das Format kennen.

  Empfehlung: Das Target-Format in eine private statische Hilfsmethode `BuildCredentialTarget(int applicationId, AuthenticationType authType)` auslagern, damit es bei einer Änderung nur an einer Stelle angepasst werden muss.

- **Fehlerbehandlung — Hardcodierter Content-Type** — In `BuildRequest` (Zeile 119): Der Body wird immer mit `"application/json"` als Fallback-Content-Type gesendet, wenn kein `Content-Type`-Header konfiguriert ist. Dieses Verhalten ist korrekt, jedoch ist der Fallback-Wert als Magic String eingebettet.

  Empfehlung: Den Fallback-Wert `"application/json"` in eine Konstante `DefaultContentType` extrahieren.

### AppDbContext.cs (AppDbContext)

- **Fehlerbehandlung — `SaveChanges` nicht überschrieben** — `SaveChangesAsync` (Zeile 17) wird überschrieben, um `RowVersion`-Werte zu aktualisieren. Das synchrone `SaveChanges` wird jedoch nicht überschrieben. Wird `SaveChanges` direkt aufgerufen (z. B. aus Tests oder Framework-Code), werden `RowVersion`-Felder nicht gesetzt, was zu inkonsistentem Concurrency-Verhalten führt.

  Empfehlung: `SaveChanges(bool acceptAllChangesOnSuccess)` ebenfalls überschreiben und `UpdateRowVersions()` dort aufrufen.

### ApplicationRepository.cs (ApplicationRepository)

- **Doppelter Code** — Die StorageMode-Filterlogik (`if (storageMode == StorageMode.User) query = query.Where(...)`) kommt identisch in `GetGroupsAsync` (Zeile 21–22), `GetApplicationsAsync` (Zeile 62–63) und `GetUngroupedApplicationsAsync` (Zeile 73–74) vor.

  Empfehlung: Eine private statische Hilfsmethode `ApplyOwnerFilter(IQueryable<Application> query, StorageMode storageMode, string owner)` extrahieren, die das Filter-Prädikat zentral anwendet und in allen drei Methoden aufgerufen wird.

### EndpointRepository.cs (EndpointRepository)

- **Doppelter Code** — Das Find-then-Remove-Muster mit `SaveChangesAsync` (inkl. Null-Check) wird identisch in `DeleteEndpointAsync` (Zeile 51–57), `DeleteEndpointGroupAsync` (Zeile 89–95), `DeleteHeaderAsync` (Zeile 107–112) und `DeleteQueryParameterAsync` (Zeile 123–129) wiederholt.

  Empfehlung: Eine generische private Hilfsmethode `DeleteByIdAsync<T>(DbSet<T>, int id)` extrahieren, um die vier Implementierungen zu konsolidieren.

### ImportDiffCalculator.cs (ImportDiffCalculator)

- **Fehlerbehandlung — fehlende Absicherung gegen Duplikate** — Wenn `imported` doppelte Schlüssel (`Method:RelativePath`) enthält, wirft `ToDictionary` (Zeile 11) eine `ArgumentException` ohne aussagekräftigen Kontext für den Aufrufer. Dies kann bei fehlerhaften Swagger- oder OData-Quellen auftreten.

  Empfehlung: Vor dem `ToDictionary`-Aufruf auf Duplikate prüfen und im Duplikat-Fall eine `ImportDiff` mit einer aussagekräftigen `ErrorMessage` zurückgeben (analog zur Fehlerbehandlung in `SwaggerImportService` und `ODataImportService`).

- **Fehlende Kapselung — eingeschränkte Diff-Erkennung** — Der Vergleich für „geändert" in `Calculate` (Zeile 17) prüft ausschließlich `Name != Name`. Änderungen an `Body`, `AuthenticationType`, Headern oder Query-Parametern werden nicht als Änderung erkannt.

  Empfehlung: Die Änderungserkennung in eine eigene private Methode `HasChanged(Endpoint existing, Endpoint imported)` extrahieren und dort alle relevanten Felder vergleichen (mind. `Body` und `AuthenticationType`), sodass die Vergleichskriterien zentral gepflegt werden können.

### SwaggerImportService.cs (SwaggerImportService)

- **Toter Code — unnötige Hilfsmethode** — `FetchStreamAsync` (Zeile 70–73) ist eine einzeilige private Methode, die nur an einer Stelle aufgerufen wird und keine eigenständige Logik kapselt. Sie fügt eine indirekte Schicht hinzu, ohne Mehrwert zu liefern.

  Empfehlung: `FetchStreamAsync` entfernen und den `_httpClientFactory.CreateClient().GetStreamAsync(url)`-Aufruf direkt in `ImportAsync` inline setzen.

- **Hardcodierter Fallback** — `MapHttpMethod` (Zeile 87) gibt bei unbekannten Methoden `GET` zurück. Da Swagger-Definitionen standardmäßig nur valide HTTP-Methoden liefern, ist dieser Fallback unerreichbar und maskiert potenziell Fehler.

  Empfehlung: Den `_ => Core.Enums.HttpMethod.GET`-Fallback durch `throw new ArgumentOutOfRangeException(nameof(method), method, null)` ersetzen, damit unbekannte Methoden sichtbar werden.

### WindowsCurrentUserService.cs (WindowsCurrentUserService)

- **Fehlerbehandlung — IDisposable nicht disposed** — `WindowsIdentity.GetCurrent()` (Zeile 10) gibt eine `WindowsIdentity`-Instanz zurück, die `IDisposable` implementiert. Der Rückgabewert wird sofort mit `.Name` ausgelesen, aber nicht disposed.

  Empfehlung: Den Aufruf in einen `using`-Block einschließen: `using var identity = WindowsIdentity.GetCurrent(); return identity.Name;`

### EndpointList.razor (EndpointList)

- **Fehlende Kapselung** — Die Filterung ungegruppierter Endpunkte `_endpoints.Where(e => e.EndpointGroupId == null).ToList()` (Zeile 36) wird inline in `OnParametersSetAsync` durchgeführt. Dasselbe logische Konzept (ungegruppierte Datensätze) existiert bereits im `ApplicationRepository` als eigene Methode `GetUngroupedApplicationsAsync`.

  Empfehlung: Eine Methode `GetUngroupedEndpointsAsync(int applicationId)` in `IEndpointRepository` und `EndpointRepository` analog zu `GetUngroupedApplicationsAsync` einführen, oder die In-Memory-Filterung in eine private Methode `FilterUngroupedEndpoints()` in der Komponente auslagern.

### EndpointExecutionPanel.razor (EndpointExecutionPanel)

- **Fehlende Aktualisierung nach Bearbeitung** — `OnEditorSaved` (Zeile 75–79) ruft `StateHasChanged()` auf, aktualisiert aber nicht das `Endpoint`-Objekt. Das UI zeigt nach dem Speichern weiterhin den veralteten Endpunktstatus an, bis die Seite neu geladen wird.

  Empfehlung: Einen `EventCallback OnEndpointUpdated` ergänzen, über den die übergeordnete `EndpointList`-Komponente informiert wird, die Endpunktliste neu zu laden, oder das `Endpoint`-Objekt nach dem Speichern durch einen erneuten Repository-Aufruf aktualisieren.

### ImportDialog.razor (ImportDialog)

- **Fehlende Kapselung — Repository-Aufruf in UI-Komponente** — `ApplyAsync` (Zeile 93–113) führt direkt Repository-Operationen (`AddEndpointAsync`, `UpdateEndpointAsync`, `DeleteEndpointAsync`) durch. UI-Komponenten sollten keine Repository-Aufrufe enthalten; das verletzt die Trennung von Darstellung und Datenzugriff.

  Empfehlung: Die Import-Anwendungslogik in `ISwaggerImportService` und `IODataImportService` als neue Methode `ApplyDiffAsync(ImportDiff diff)` verlagern. `ImportDialog` gibt den selektierten `ImportDiff` über einen `EventCallback<ImportDiff>` zurück und ruft keinen Repository direkt auf.

### EndpointRepositoryIntegrationTests.cs (EndpointRepositoryIntegrationTests)

- **Fehlerbehandlung — synchrones `using` für async-disposable Kontexte** — `context1` (Zeile 23) und `context2` (Zeile 40) werden mit `using var` (synchron) statt `await using var` deklariert. `AppDbContext` implementiert `IAsyncDisposable`. Synchrones Disposal ruft die synchrone `Dispose`-Methode auf, die async-Aufräumarbeiten nicht korrekt abwartet.

  Empfehlung: `using var context1` und `using var context2` durch `await using var context1` und `await using var context2` ersetzen, analog zur Behandlung in `ApplicationRepositoryIntegrationTests.cs`.

### EndpointExecutionServiceTests.cs (EndpointExecutionServiceTests)

- **Testqualität — Test prüft nicht das erwartete Verhalten** — `Execute_WithAuthTypeNegotiate_UsesNegotiateHandler` (Zeile 91–103) verifiziert nicht, dass der `HttpClientFactory` mit dem Namen `"negotiate"` aufgerufen wurde. Stattdessen prüft er nur `result.Success == false`, was kein fachlich aussagekräftiges Verhalten ist.

  Empfehlung: Den Test auf `factoryMock` umstellen (statt `new HttpClient()` direkt) und `factoryMock.Verify(f => f.CreateClient("negotiate"), Times.Once())` als Assertion hinzufügen.

- **Testqualität — Test prüft nicht das erwartete Verhalten** — `Execute_WithAuthTypeNegotiateWithImpersonation_RunsImpersonated` (Zeile 125–139) assertiert nur `Assert.NotNull(result)`, was keine fachliche Aussage trifft. Das im Plan genannte Ziel — „`WindowsIdentity.RunImpersonated` wird aufgerufen" — wird nicht überprüft.

  Empfehlung: `factoryMock.Verify(f => f.CreateClient("negotiate"), Times.Once())` als primäre Assertion verwenden und den Test-Namen auf das tatsächlich geprüfte Verhalten angleichen.

### ODataImportServiceTests.cs (ODataImportServiceTests)

- **Namenskonvention — Inkonsistenz** — Der dritte Test heißt `Import_WithRemovedEndpoint_ReturnsRemovedInDiff` (Zeile 85), während die anderen Tests dem Muster `Import_{Zustand}_{Ergebnis}` folgen (`Import_NewODataMetadata_ReturnsCorrectDiff`, `Import_ChangedODataMetadata_ReturnsChangedInDiff`). Das abweichende Präfix `With` bricht die Einheitlichkeit.

  Empfehlung: Testmethode umbenennen zu `Import_RemovedODataEndpoint_ReturnsRemovedInDiff`, entsprechend dem Muster der anderen Tests in dieser Klasse.

## Geprüfte Dateien

- `src/Schnittstellenzentrale.Core/Enums/AuthenticationType.cs`
- `src/Schnittstellenzentrale.Core/Enums/HttpMethod.cs`
- `src/Schnittstellenzentrale.Core/Enums/StorageMode.cs`
- `src/Schnittstellenzentrale.Core/Interfaces/IApplicationRepository.cs`
- `src/Schnittstellenzentrale.Core/Interfaces/ICredentialService.cs`
- `src/Schnittstellenzentrale.Core/Interfaces/ICurrentUserService.cs`
- `src/Schnittstellenzentrale.Core/Interfaces/IEndpointExecutionService.cs`
- `src/Schnittstellenzentrale.Core/Interfaces/IEndpointRepository.cs`
- `src/Schnittstellenzentrale.Core/Interfaces/IHealthCheckService.cs`
- `src/Schnittstellenzentrale.Core/Interfaces/IODataImportService.cs`
- `src/Schnittstellenzentrale.Core/Interfaces/ISignalRNotificationService.cs`
- `src/Schnittstellenzentrale.Core/Interfaces/IStorageModeService.cs`
- `src/Schnittstellenzentrale.Core/Interfaces/ISwaggerImportService.cs`
- `src/Schnittstellenzentrale.Core/Models/Application.cs`
- `src/Schnittstellenzentrale.Core/Models/ApplicationGroup.cs`
- `src/Schnittstellenzentrale.Core/Models/Endpoint.cs`
- `src/Schnittstellenzentrale.Core/Models/EndpointExecutionResult.cs`
- `src/Schnittstellenzentrale.Core/Models/EndpointGroup.cs`
- `src/Schnittstellenzentrale.Core/Models/EndpointHeader.cs`
- `src/Schnittstellenzentrale.Core/Models/EndpointQueryParameter.cs`
- `src/Schnittstellenzentrale.Core/Models/ImportDiff.cs`
- `src/Schnittstellenzentrale.Infrastructure/Data/AppDbContext.cs`
- `src/Schnittstellenzentrale.Infrastructure/Data/AppDbContextFactory.cs`
- `src/Schnittstellenzentrale.Infrastructure/Data/DatabaseProviderFactory.cs`
- `src/Schnittstellenzentrale.Infrastructure/Data/Migrations/20260514070438_InitialCreate.cs`
- `src/Schnittstellenzentrale.Infrastructure/Data/SqlServerMigrations/20260514000000_InitialCreate.cs`
- `src/Schnittstellenzentrale.Infrastructure/Repositories/ApplicationRepository.cs`
- `src/Schnittstellenzentrale.Infrastructure/Repositories/EndpointRepository.cs`
- `src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionService.cs`
- `src/Schnittstellenzentrale.Infrastructure/Services/HealthCheckService.cs`
- `src/Schnittstellenzentrale.Infrastructure/Services/ImportDiffCalculator.cs`
- `src/Schnittstellenzentrale.Infrastructure/Services/ODataImportService.cs`
- `src/Schnittstellenzentrale.Infrastructure/Services/SignalRNotificationService.cs`
- `src/Schnittstellenzentrale.Infrastructure/Services/StorageModeService.cs`
- `src/Schnittstellenzentrale.Infrastructure/Services/SwaggerImportService.cs`
- `src/Schnittstellenzentrale.Infrastructure/Services/WindowsCredentialService.cs`
- `src/Schnittstellenzentrale.Infrastructure/Services/WindowsCurrentUserService.cs`
- `src/Schnittstellenzentrale.Tests/Helpers/TestHelpers.cs`
- `src/Schnittstellenzentrale.Tests/Integration/ApplicationRepositoryIntegrationTests.cs`
- `src/Schnittstellenzentrale.Tests/Integration/EndpointRepositoryIntegrationTests.cs`
- `src/Schnittstellenzentrale.Tests/Services/DatabaseProviderFactoryTests.cs`
- `src/Schnittstellenzentrale.Tests/Services/EndpointExecutionServiceTests.cs`
- `src/Schnittstellenzentrale.Tests/Services/HealthCheckServiceTests.cs`
- `src/Schnittstellenzentrale.Tests/Services/ODataImportServiceTests.cs`
- `src/Schnittstellenzentrale.Tests/Services/SwaggerImportServiceTests.cs`
- `src/Schnittstellenzentrale/Components/Pages/Home.razor`
- `src/Schnittstellenzentrale/Components/Shared/ApplicationCard.razor`
- `src/Schnittstellenzentrale/Components/Shared/ApplicationGroupTree.razor`
- `src/Schnittstellenzentrale/Components/Shared/CollapsibleSection.razor`
- `src/Schnittstellenzentrale/Components/Shared/ConcurrencyWarningDialog.razor`
- `src/Schnittstellenzentrale/Components/Shared/EndpointEditor.razor`
- `src/Schnittstellenzentrale/Components/Shared/EndpointExecutionPanel.razor`
- `src/Schnittstellenzentrale/Components/Shared/EndpointList.razor`
- `src/Schnittstellenzentrale/Components/Shared/HealthCheckDialog.razor`
- `src/Schnittstellenzentrale/Components/Shared/ImportDialog.razor`
- `src/Schnittstellenzentrale/Components/Shared/ODataImportDialog.razor`
- `src/Schnittstellenzentrale/Components/Shared/SwaggerImportDialog.razor`
- `src/Schnittstellenzentrale/Components/_Imports.razor`
- `src/Schnittstellenzentrale/Components/Layout/MainLayout.razor`
- `src/Schnittstellenzentrale/Components/Layout/NavMenu.razor`
- `src/Schnittstellenzentrale/Hubs/EndpointHub.cs`
- `src/Schnittstellenzentrale/Program.cs`
- `src/Schnittstellenzentrale/appsettings.json`
