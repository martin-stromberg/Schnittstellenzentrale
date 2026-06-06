# Bestandsaufnahme: OData-API (Issue #48)

Analysiert wurden die Bereiche Controller, Datenmodell, Interfaces, Enums, Infrastruktur-Services und Tests des Projekts `Schnittstellenzentrale` bezogen auf die Anforderung, einen OData v4-Endpunkt unter `/odatav4` bereitzustellen.

---

## Zusammenfassung

- Die vier Kerndatenmodelle (`Application`, `ApplicationGroup`, `Endpoint`, `EndpointGroup`) sind vollständig definiert und besitzen alle für das OData-EDM-Modell benötigten Navigationseigenschaften (`Application.Endpoints`, `Application.EndpointGroups`, `Application.ApplicationGroup`, `EndpointGroup.ParentGroup`, `EndpointGroup.ChildGroups`).
- `Application.DetectInterfaceType` erkennt bereits `$metadata`-URLs als `InterfaceType.OData`.
- Die vier REST-Controller (`ApplicationsController`, `ApplicationGroupsController`, `EndpointsController`, `EndpointGroupsController`) existieren vollständig und dienen als direkte Implementierungsvorlage für die OData-Gegenstücke.
- `ApiControllerBase` kapselt die gesamte Token-Validierungslogik (`ITokenStore.ValidateAndRotateAsync`) und die DTO-Mapping-Methoden; OData-Controller müssen entweder von ihr erben oder eine eigene `ODataControllerBase` einführen.
- `IODataImportService` und `ODataImportService` existieren bereits und parsen CSDL-Metadaten via `Microsoft.OData.Edm` (`CsdlReader`). Das Paket ist in `Schnittstellenzentrale.Infrastructure.csproj` referenziert.
- **Das Paket `Microsoft.AspNetCore.OData` fehlt** in `Schnittstellenzentrale.csproj` vollständig — es ist noch nicht referenziert.
- In `Program.cs` fehlen: `AddOData()`-Registrierung, `MapODataRoute()`-Konfiguration und ein `ODataEdmModelBuilder`.
- `SystemEndpointSyncService` nutzt ausschließlich `ISwaggerImportService` und `ISwaggerProvider`; er registriert keine OData-Route und muss nicht angepasst werden.
- In `PlaywrightServer` ist `ISwaggerImportService` gemockt; `IODataImportService` läuft bereits unggemockt über den `inProcessHandler` — kein zusätzlicher Mock-Aufwand für OData-Playwright-Tests.
- Der `ControllerTestFactory` ist die bestehende `WebApplicationFactory` für Controller-Integrationstests und kann ohne strukturelle Änderung für `ODataControllerIntegrationTests` genutzt werden.
- **Fehlende Tests:** `ODataImportServiceRealMetadataTests`, `ODataControllerIntegrationTests`, `ODataImportTests` (Playwright) sowie Unit-Tests für die OData-Controller-CRUD-Logik sind noch nicht vorhanden.

---

## Details

- [Datenmodell](inventory/models.md)
- [Logik](inventory/logic.md)
- [Enums](inventory/enums.md)
- [Interfaces](inventory/interfaces.md)
- [Tests](inventory/tests.md)
