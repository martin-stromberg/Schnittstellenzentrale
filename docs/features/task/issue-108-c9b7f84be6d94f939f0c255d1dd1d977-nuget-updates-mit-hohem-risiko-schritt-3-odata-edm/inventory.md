# Bestandsaufnahme – Schritt 3 (F5): Microsoft.OData.Edm 9.0.0 + AspNetCore.OData-Strategie

Datum: 2026-04-27 · Basis: Basisbranch `…-nuget-updates-mit-hohem-risiko` (Schritte 1+2 gemergt)

## Paketlage (verifiziert via `dotnet list package --include-transitive`)

| Projekt | Direkte Referenz | Transitive Auflösung |
|---------|------------------|----------------------|
| `src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj:36` | `Microsoft.OData.Edm 8.4.3` | `Microsoft.OData.Core 9.0.0-preview.4`, `Microsoft.OData.Edm 9.0.0-preview.4`, `Microsoft.OData.ModelBuilder 3.0.0-preview.1` (via `Microsoft.AspNetCore.OData 10.0.0-preview.2` im Web-Projekt) |
| `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj:16` | `Microsoft.AspNetCore.OData 10.0.0-preview.2` | `Microsoft.OData.Core 9.0.0-preview.4`, `Microsoft.OData.Edm 9.0.0-preview.4`, `Microsoft.OData.ModelBuilder 3.0.0-preview.1` |

**Bestätigter Versions-Split:** Infrastructure kompiliert gegen Edm `8.4.3`, läuft aber transitiv gegen `9.0.0-preview.4` (binärfragil — genau der im Issue beschriebene Befund F5).

**Verfügbarkeit geprüft:** `Microsoft.OData.Edm 9.0.0` (stabil) existiert auf nuget.org (`dotnet package search`). `9.0.0` (stabil) > `9.0.0-preview.4` → erfüllt die AspNetCore.OData-Mindestanforderung `>= 9.0.0-preview.4`; NuGet löst einheitlich auf `9.0.0` auf. Kein stabiler `Microsoft.AspNetCore.OData` 10.x/11.x existiert (neueste: `10.0.0-preview.2`; letzter stabiler Strang 9.x für net8.0).

## Edm-API-Nutzung im Code (vollständig erfasst)

**Infrastructure** — `src/Schnittstellenzentrale.Infrastructure/Services/ODataImportService.cs`:
- `CsdlReader.Parse(XmlReader)` (Z. 64, using `Microsoft.OData.Edm.Csdl`, `Microsoft.OData.Edm`)
- `IEdmModel` (Z. 59), `model.EntityContainer?.EntitySets()` (Z. 87), `IEdmEntitySet` (Z. 87), `entitySet.Name`
- `model.SchemaElements.OfType<IEdmOperation>()` (Z. 106), `operation is IEdmAction` (Z. 108), `operation.Name`
- Rest des Services arbeitet auf `System.Xml.Linq` (Annotationen werden manuell aus dem XML gelesen — unabhängig von der Edm-API).

**Web** — `src/Schnittstellenzentrale/OData/ODataEdmModelBuilder.cs` (kompiliert gegen transitive Edm-Version):
- `ODataConventionModelBuilder` (`Microsoft.OData.ModelBuilder`), `builder.EntitySet<T>()`, `HasKey`, `Ignore`, `builder.Action()`, `Returns<T>()`, `GetEdmModel()`
- `EdmModel`, `EdmTerm` (Z. 47), `EdmCoreModel.Instance.GetString(true)`, `model.AddElement`, `model.SetVocabularyAnnotation`, `EdmVocabularyAnnotation`, `EdmStringConstant` (alle `Microsoft.OData.Edm`/`Microsoft.OData.Edm.Vocabularies`)
- `model.EntityContainer?.FindEntitySet(name)` (Z. 61), `model.SchemaElements.OfType<IEdmAction>()` (Z. 71–73)

Keine weiteren `Microsoft.OData.*`-usings in `Core` oder `Infrastructure` außerhalb `ODataImportService.cs` (Volltextsuche über `src/`).

## OData-Laufzeitpfad

- `src/Schnittstellenzentrale/Program.cs:80–87`: `AddOData(o => o.AddRouteComponents("odatav4", ODataEdmModelBuilder.Build()).Select().Filter().Expand().OrderBy().Count().SetMaxTop(1000))`.
- Controller unter `src/Schnittstellenzentrale/OData/`: `ODataApplicationsController`, `ODataApplicationGroupsController`, `ODataEndpointsController`, `ODataEndpointGroupsController` (alle `[EnableQuery]`, Basisklasse `ODataControllerBase`), `ODataAuthController` (`/odatav4/Authenticate()`, `[Authorize]` + Negotiate → Bearer-Token), `ODataControllerBase` (Bearer-Token-Validierung per `IAsyncActionFilter`).
- Authentifizierung: `ODataAuthController` per Negotiate (`AddAuthentication(NegotiateDefaults.AuthenticationScheme)`, `Program.cs:65–67`); OData-GETs benötigen `Authorization: Bearer <token>` (`ODataControllerBase.OnActionExecutionAsync`).

## Testabdeckung (reguläre Lane)

| Datei | Inhalt | Tests |
|-------|--------|-------|
| `src/Schnittstellenzentrale.Tests/Integration/ODataControllerIntegrationTests.cs` | `WebApplicationFactory`-Tests gegen `[EnableQuery]`-Endpunkte (`/odatav4/...`), inkl. Query-Optionen, PATCH/POST/DELETE, `$metadata` | 78 |
| `src/Schnittstellenzentrale.Tests/Integration/ODataImportServiceIntegrationTests.cs` | `IODataImportService` mit realer In-Memory-DB + gemocktem `HttpMessageHandler` | 1 |
| `src/Schnittstellenzentrale.Tests/Integration/ODataControllerNullApplicationTests.cs` | OData-Controller, Null-Anwendung | klein |
| `src/Schnittstellenzentrale.Tests/Integration/ODataNotificationIntegrationTests.cs` | SignalR-Notification bei OData-Operationen | klein |
| `src/Schnittstellenzentrale.Tests/Services/ODataImportServiceTests.cs` | Unit-Tests `ODataImportService` (CsdlReader-Pfad, Annotationen, Auth-Typen) | 23 |
| `src/Schnittstellenzentrale.Tests/Services/ODataImportServiceRealMetadataTests.cs` | Reale `$metadata`-Dokumente | 1 |
| `src/Schnittstellenzentrale.Tests/Integration/ApplicationImportControllerIntegrationTests.cs` + `ImportApplyNotificationTests.cs` | Import-Controller-Pfad | mehrere |

Baseline reguläre Lane (`Category!=OsInterface`): **641 Tests** (aus Abnahme Schritt 2 verifiziert). OsInterface-Lane ist in diesem Schritt nicht erforderlich.

## E2E-Abnahme-Weg

- Start: `dotnet run --project src/Schnittstellenzentrale --launch-profile http` → Kestrel `http://localhost:5128` (`Properties/launchSettings.json`).
- Auth-Flow: `GET /odatav4/Authenticate()` mit Windows-Default-Credentials (Negotiate) → JSON `{Token}`; dann `GET /odatav4/Applications` mit `Authorization: Bearer <token>` → 200 JSON. Alternativ `GET /odatav4/$metadata` für CSDL-Ausgabe des ModelBuilders.
- Windows-Umgebung vorhanden → `Invoke-RestMethod -UseDefaultCredentials` bzw. `curl --negotiate -u :` nutzbar.

## Dokumentationsziele

- `docs/projects/task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko/decisions.md` — Abschnitt F5 ergänzen (Struktur wie F6-Abschnitt).
- `docs/RELEASE_NOTES.md` — Eintrag analog Schritt 2.
- `docs/help/` — prüfen, ob ein NuGet-/Update-Hilfethema existiert; Edm-9-Voraussetzung ist primär Build-Detail, kein Endanwender-Feature → vermutlich kein neues Hilfethema nötig (Entscheidung in Schritt 12).

## Risiken

- **Edm-9-API-Breaks:** `CsdlReader.Parse`, Vocabulary-Annotation-APIs (`EdmVocabularyAnnotation`, `SetVocabularyAnnotation`, `EdmTerm`) könnten in 9.0 geändert/entfernt worden sein → Build-Fehler zeigen Befund; ggf. Codeanpassung in `ODataEdmModelBuilder`/`ODataImportService`.
- **Transitive Kohärenz:** `Microsoft.OData.Core 9.0.0-preview.4`/`ModelBuilder 3.0.0-preview.1` müssen mit Edm 9.0.0 (stabil) binär kompatibel sein — ODL-9-Linie, erwartete Kompatibilität; Verifikation über 78 Controller-Integrationstests + E2E.
- **Fallback:** Bei nicht behebbaren Problemen `8.4.4` (dann bleibt der Split-Doc-Hinweis relevant, Entscheidung dokumentieren).
