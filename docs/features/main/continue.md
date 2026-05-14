# Offene Aufgaben (Code-Review)

Bewusst zurückgestellte Befunde aus dem Code-Review. Plan-Review ist grün (Vollständig umgesetzt).

---

## Ausnahemfehler beim Programmstart

Microsoft.Data.Sqlite.SqliteException
  HResult=0x80004005
  Nachricht = SQLite Error 1: 'no such table: ApplicationGroups'.
  Quelle = Microsoft.Data.Sqlite
  Stapelüberwachung:
   bei Microsoft.Data.Sqlite.SqliteException.ThrowExceptionForRC(Int32 rc, sqlite3 db)
   bei Microsoft.Data.Sqlite.SqliteCommand.<PrepareAndEnumerateStatements>d__64.MoveNext()
   bei Microsoft.Data.Sqlite.SqliteCommand.<GetStatements>d__54.MoveNext()
   bei Microsoft.Data.Sqlite.SqliteDataReader.NextResult()
   bei Microsoft.Data.Sqlite.SqliteCommand.ExecuteReader(CommandBehavior behavior)
   bei Microsoft.Data.Sqlite.SqliteCommand.ExecuteReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
   bei Microsoft.Data.Sqlite.SqliteCommand.<ExecuteDbDataReaderAsync>d__60.MoveNext()
   bei Microsoft.EntityFrameworkCore.Storage.RelationalCommand.<ExecuteReaderAsync>d__18.MoveNext()
   bei Microsoft.EntityFrameworkCore.Storage.RelationalCommand.<ExecuteReaderAsync>d__18.MoveNext()
   bei Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.<InitializeReaderAsync>d__21.MoveNext()
   bei Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.<MoveNextAsync>d__20.MoveNext()
   bei System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable`1.ConfiguredValueTaskAwaiter.GetResult()
   bei Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.<ToListAsync>d__67`1.MoveNext()
   bei Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.<ToListAsync>d__67`1.MoveNext()
   bei Schnittstellenzentrale.Infrastructure.Repositories.ApplicationRepository.<GetGroupsAsync>d__2.MoveNext() in C:\Users\Martin\Documents\Repositories\Schnittstellenzentrale\src\Schnittstellenzentrale.Infrastructure\Repositories\ApplicationRepository.cs: Zeile23
   bei Schnittstellenzentrale.Components.Shared.ApplicationGroupTree.<LoadDataAsync>d__9.MoveNext() in C:\Users\Martin\Documents\Repositories\Schnittstellenzentrale\src\Schnittstellenzentrale\Components\Shared\ApplicationGroupTree.razor: Zeile55
   bei Schnittstellenzentrale.Components.Shared.ApplicationGroupTree.<OnInitializedAsync>d__8.MoveNext() in C:\Users\Martin\Documents\Repositories\Schnittstellenzentrale\src\Schnittstellenzentrale\Components\Shared\ApplicationGroupTree.razor: Zeile50
   bei Microsoft.AspNetCore.Components.ComponentBase.<RunInitAndSetParametersAsync>d__28.MoveNext()
   bei Microsoft.AspNetCore.Components.Rendering.ComponentState.SetDirectParameters(ParameterView parameters)
   bei Microsoft.AspNetCore.Components.RenderTree.RenderTreeDiffBuilder.InitializeNewComponentFrame(DiffContext& diffContext, Int32 frameIndex)
   bei Microsoft.AspNetCore.Components.RenderTree.RenderTreeDiffBuilder.InitializeNewSubtree(DiffContext& diffContext, Int32 frameIndex)
   bei Microsoft.AspNetCore.Components.RenderTree.RenderTreeDiffBuilder.InsertNewFrame(DiffContext& diffContext, Int32 newFrameIndex)
   bei Microsoft.AspNetCore.Components.RenderTree.RenderTreeDiffBuilder.AppendDiffEntriesForRange(DiffContext& diffContext, Int32 oldStartIndex, Int32 oldEndIndexExcl, Int32 newStartIndex, Int32 newEndIndexExcl)
   bei Microsoft.AspNetCore.Components.RenderTree.RenderTreeDiffBuilder.ComputeDiff(Renderer renderer, RenderBatchBuilder batchBuilder, Int32 componentId, ArrayRange`1 oldTree, ArrayRange`1 newTree)
   bei Microsoft.AspNetCore.Components.Rendering.ComponentState.RenderIntoBatch(RenderBatchBuilder batchBuilder, RenderFragment renderFragment, Exception& renderFragmentException)
   bei Microsoft.AspNetCore.Components.RenderTree.Renderer.ProcessRenderQueue()

Es muss durch weitere Tests sichergestellt werden, dass beim programmstart die Datenbank korrekt initialisirt wird, falls notwendig.

## EndpointExecutionService.cs

- **Doppelter Code** — `ExecuteWithAuthAsync` und `ExecuteImpersonatedAsync` duplizieren das Request-Build-and-Send-Muster. Gemeinsame Hilfsmethode `SendAndBuildResultAsync(HttpClient, Endpoint)` extrahieren.
- **Fehlerbehandlung** — `catch (Exception ex)` fängt `OperationCanceledException`. Vor dem generellen Block explizit behandeln oder weiterwerfen.
- **Magic String** — Credential-Target-Format als Literal. In private statische Methode `BuildCredentialTarget(int applicationId, AuthenticationType authType)` auslagern.
- **Konstante** — Fallback `"application/json"` als `DefaultContentType`-Konstante extrahieren.

## AppDbContext.cs

- **Fehlerbehandlung** — Synchrones `SaveChanges` nicht überschrieben; `RowVersion` wird dabei nicht gesetzt. `SaveChanges(bool acceptAllChangesOnSuccess)` analog zu `SaveChangesAsync` überschreiben.

## ApplicationRepository.cs

- **Doppelter Code** — StorageMode-Filterlogik dreifach wiederholt. Private statische Hilfsmethode `ApplyOwnerFilter(IQueryable<Application>, StorageMode, string)` extrahieren.

## EndpointRepository.cs

- **Doppelter Code** — Find-then-Remove-Muster viermal identisch. Generische private Hilfsmethode `DeleteByIdAsync<T>(DbSet<T>, int id)` extrahieren.

## ImportDiffCalculator.cs

- **Fehlerbehandlung** — `ToDictionary` wirft `ArgumentException` bei Duplikat-Schlüsseln. Vorher prüfen, bei Duplikat `ImportDiff` mit `ErrorMessage` zurückgeben.
- **Eingeschränkte Diff-Erkennung** — Änderungserkennung prüft nur `Name`. Private Methode `HasChanged(Endpoint existing, Endpoint imported)` mit vollständigem Feldvergleich (mind. `Body`, `AuthenticationType`) einführen.

## SwaggerImportService.cs

- **Toter Code** — `FetchStreamAsync` ist eine einzeilige Hilfsmethode ohne Mehrwert. Entfernen und Aufruf inline setzen.
- **Verdeckter Fehler** — `MapHttpMethod` gibt bei unbekannten Methoden `GET` zurück. Durch `throw new ArgumentOutOfRangeException(...)` ersetzen.

## WindowsCurrentUserService.cs

- **IDisposable nicht disposed** — `WindowsIdentity.GetCurrent()` in `using var identity = ...`-Block einschließen.

## EndpointList.razor

- **Fehlende Kapselung** — Ungegruppierte Endpunkte werden inline gefiltert. Entweder `GetUngroupedEndpointsAsync(int applicationId)` in `IEndpointRepository` einführen oder in private Methode auslagern.

## EndpointExecutionPanel.razor

- **Veralteter Zustand nach Speichern** — `OnEditorSaved` ruft nur `StateHasChanged()`, aktualisiert aber nicht das `Endpoint`-Objekt. `EventCallback OnEndpointUpdated` ergänzen oder Endpunkt per Repository neu laden.

## ImportDialog.razor

- **Repository-Aufruf in UI** — `ApplyAsync` greift direkt auf Repositories zu. Logik als `ApplyDiffAsync(ImportDiff diff)` in `ISwaggerImportService`/`IODataImportService` verlagern; Dialog gibt Diff per `EventCallback<ImportDiff>` zurück.

## EndpointRepositoryIntegrationTests.cs

- **Synchrones Disposal für async-disposable Kontext** — `using var context1/context2` auf `await using var` umstellen.

## EndpointExecutionServiceTests.cs

- **Testqualität** — `Execute_WithAuthTypeNegotiate_UsesNegotiateHandler` prüft nur `result.Success == false`. `factoryMock.Verify(f => f.CreateClient("negotiate"), Times.Once())` als Assertion ergänzen.
- **Testqualität** — `Execute_WithAuthTypeNegotiateWithImpersonation_RunsImpersonated` assertiert nur `Assert.NotNull(result)`. Auf `factoryMock.Verify(...)` umstellen.

## ODataImportServiceTests.cs

- **Namenskonvention** — `Import_WithRemovedEndpoint_ReturnsRemovedInDiff` umbenennen zu `Import_RemovedODataEndpoint_ReturnsRemovedInDiff`.
