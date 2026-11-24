# Code-Verwendung der prüfrelevanten Pakete

Recherche via ripgrep auf `src/` (Stand 2026-11-23). Nur Fakten — ob Referenzen entfernbar sind, wird nicht bewertet.

## Kontext: Framework-Bezüge

- `Schnittstellenzentrale` (Web): SDK `Microsoft.NET.Sdk.Web` → implizite `FrameworkReference` auf `Microsoft.AspNetCore.App` (aufgelöst als `Microsoft.AspNetCore.App.Internal.Assets [10.0.15, )`).
- `Schnittstellenzentrale.Core`, `Schnittstellenzentrale.Infrastructure`, `Schnittstellenzentrale.Tests`: SDK `Microsoft.NET.Sdk` — **keine** `FrameworkReference` auf `Microsoft.AspNetCore.App` gefunden (kein Treffer für `FrameworkReference`/`Microsoft.AspNetCore.App` in allen csproj). ASP.NET-Typen in diesen Projekten kommen ausschließlich aus NuGet-Paketen.

## `Microsoft.AspNetCore.Http.Abstractions` 2.3.0 (Infrastructure)

Einziger Verwendungsnachweis im Infrastructure-Code:

- `src/Schnittstellenzentrale.Infrastructure/Services/WindowsCurrentUserService.cs` — `using Microsoft.AspNetCore.Http;` (Z. 2), Feld `IHttpContextAccessor _httpContextAccessor` (Z. 12), Konstruktorparameter (Z. 17), Zugriff `_httpContextAccessor.HttpContext?.User?.Identity` (Z. 27). Kommentar im ctor-Doc (Z. 14): der übergebene Accessor werde „nicht verwendet" (Windows-Identität via `WindowsIdentity.GetCurrent()`), der Typ bleibt aber im Konstruktor-Contract.
- Registrierung: `src/Schnittstellenzentrale/Program.cs` Z. 140 (`AddSingleton<ICurrentUserService, WindowsCurrentUserService>()`), Z. 143 (`AddHttpContextAccessor()`).
- Weitere `IHttpContextAccessor`-Verwendungen (Web-Projekt, aus Shared Framework, nicht aus dem Paket): `Services/ApplicationApiClient.cs` Z. 16/33, `Components/App.razor` Z. 1; Test-Mock in `Tests/Services/ApplicationApiClientTests.cs` Z. 50.
- Transitiv zieht das Paket `Microsoft.AspNetCore.Http.Features` 2.3.0 (outdated → 5.0.17).
- Sonstige `Http*`-Treffer in Infrastructure (`HttpRequestMessage`, `HttpResponseMessage`, `HttpRequestException` in `EndpointExecutionService.cs`, `SwaggerImportService.cs`, `ODataImportService.cs`, `HealthCheckService.cs`) stammen aus `System.Net.Http`, nicht aus diesem Paket.

## `Microsoft.AspNetCore.SignalR.Core` 1.2.10 (Infrastructure)

Verwendungsnachweis:

- `src/Schnittstellenzentrale.Infrastructure/Services/SignalRNotificationService.cs` — `using Microsoft.AspNetCore.SignalR;` (Z. 1), `IHubContext<THub> _hubContext` (Z. 13), generischer Constraint auf Hub-Typ. `IHubContext<T>`/`Hub` werden aus dem Paket `SignalR.Core` bezogen (kein Framework-Bezug im Projekt).
- Instanziierung: `src/Schnittstellenzentrale/Program.cs` Z. 141 `AddScoped<ISignalRNotificationService, SignalRNotificationService<EndpointHub>>()`.
- Server-Hub (im Web-Projekt, Typen aus Shared Framework): `src/Schnittstellenzentrale/Hubs/EndpointHub.cs` — `public class EndpointHub : Hub` (Z. 6), gemappt in `Program.cs` Z. 215 `app.MapHub<EndpointHub>("/hubs/endpoint")`, `AddSignalR()` Z. 117.
- Tests verwenden `IHubContext<EndpointHub>` direkt: `Tests/Playwright/Infrastructure/PlaywrightSignalRServer.cs` Z. 1 (`using Microsoft.AspNetCore.SignalR`), Z. 29–30 (`GetRequiredService<IHubContext<EndpointHub>>()`) — Typ im Testprojekt transitiv verfügbar (über Projektverweis auf Infrastructure bzw. Web).
- Paket-Fakten: 1.2.10 ist die letzte Paketlinie für serverseitige SignalR-Typen (ab ASP.NET Core 3.x im Shared Framework); neueste auf NuGet: 1.2.14 (Patch).

## `Microsoft.AspNetCore.SignalR.Client` 10.0.* → 10.0.15 (Infrastructure)

- **Keine Verwendung im Infrastructure-Code** (kein `HubConnection`/`HubConnectionBuilder`-Treffer in `src/Schnittstellenzentrale.Infrastructure`).
- Nutzung im Web-Projekt (Typen fließen über `ProjectReference` → Infrastructure mit): `Components/Shared/ApplicationGroupTree.razor` Z. 9–10, 100, 124 (`HubConnectionBuilder`); `Components/Layout/AppShell.razor` Z. 11, 55, 135.
- Neueste verfügbare Linie: 11.0.0.

## `Microsoft.AspNetCore.OData` 10.0.0-preview.2 (Web)

Produktive Verwendung — eigener OData-API-Zweig:

- `src/Schnittstellenzentrale/Program.cs`: `using Microsoft.AspNetCore.OData;` (Z. 1), `.AddOData(o => o.AddRouteComponents("odatav4", ODataEdmModelBuilder.Build()).Select().Filter().Expand().OrderBy().Count().SetMaxTop(1000))` (Z. 80–87), Kommentar Z. 75 (`ReferenceHandler.IgnoreCycles` für OData-Zyklen), `IODataImportService`-Registrierung Z. 138.
- `src/Schnittstellenzentrale/OData/` (9 Dateien): `ODataEdmModelBuilder.cs` (`ODataConventionModelBuilder`, `(EdmModel)builder.GetEdmModel()`), `ODataControllerBase.cs` (`ControllerBase, IAsyncActionFilter`, Token-Validierung), `ODataEndpointsController.cs`, `ODataEndpointGroupsController.cs`, `ODataApplicationsController.cs`, `ODataApplicationGroupsController.cs` (je `[EnableQuery]` aus `Microsoft.AspNetCore.OData.Query`), `ODataAuthController.cs` (`[ODataAttributeRouting]` aus `Microsoft.AspNetCore.OData.Routing.Attributes`, Unbound Action/Function `authenticate`), `ODataPatchHelper.cs`, `ODataEndpointUpdateHelper.cs`.
- Transitive Preview-Abhängigkeiten (Web + Tests): `Microsoft.OData.Core`/`Edm`/`Spatial` 9.0.0-preview.4, `Microsoft.OData.ModelBuilder` 3.0.0-preview.1 — für alle vier existieren bereits stabile Releases (9.0.0 bzw. 3.0.0, s. outdated.md).
- Testabdeckung: `ODataControllerIntegrationTests` (78 `[Fact]`/`[Theory]`-Methoden), `ODataControllerNullApplicationTests`, `ODataImportServiceIntegrationTests`, `ODataNotificationIntegrationTests`, Playwright `ODataEndpointExecutionTests`, `ODataImportTests`.

## `Microsoft.OData.Edm` 8.4.3 (Infrastructure, direkt)

- `src/Schnittstellenzentrale.Infrastructure/Services/ODataImportService.cs` — `using Microsoft.OData.Edm;` + `using Microsoft.OData.Edm.Csdl;` (Z. 2–3), `IEdmModel` (Z. 59), `CsdlReader.Parse(xmlReader)` (Z. 64). Zweck: CSDL/$metadata-Import fremder OData-Services.
- Versionsinkonsistenz (Faktum): Infrastructure löst 8.4.3 auf; Web/Tests lösen transitiv 9.0.0-preview.4 auf (höhere Anforderung des `Microsoft.AspNetCore.OData`-Preview-Pakets gewinnt dort per nearest-wins).

## Weitere Paket-Verwendungsnachweise (Kontext)

| Paket | Verwendung |
|-------|-----------|
| `Jint` 4.9.2 | `Infrastructure/Services/EndpointScriptRunner.cs` (`using Jint`, `Jint.Native`, `Jint.Runtime`); direkt auch im Testprojekt referenziert (`EndpointScriptRunnerTests`) |
| `Markdig` 1.2.0 | `Infrastructure/Services/ImpressumService.cs` (`using Markdig`) |
| `AngleSharp` 1.5.0 (Tests) | Override gegen transitive 1.4.0 via bunit (die `using AngleSharp;`-Direktive in `Tests/Playwright/Infrastructure/PlaywrightServer.cs` Z. 2 ist ungenutzt — kein API-Aufruf) |
| `Moq` | z. B. `Mock<ISignalRNotificationService>` in `EndpointScriptRunnerTests`, `EndpointExecutionServiceTests`, `Mock<IHttpContextAccessor>` in `ApplicationApiClientTests` |
| `Microsoft.OpenApi` 2.10.0 | Core + Infrastructure (Swagger/OpenAPI-Dokumentverarbeitung) |
