# Bestandsaufnahme: Einheitliches Aggregat-Update für Endpunkte

Analyse des Endpunkt-Aggregat-Updates (Skalare + Kind-Mengen Header/Query-Parameter) über alle mutierenden Pfade: REST (`EndpointsController`), OData (`ODataEndpointsController`), Import-Apply (`SwaggerImportService`, `ODataImportService`) und UI (`EndpointPage.razor`).

## Zusammenfassung

- Die Aggregat-Operation existiert bereits: `EndpointRepository.UpdateEndpointAsync` (`src/Schnittstellenzentrale.Infrastructure/Repositories/EndpointRepository.cs`, Z.104–130) lädt den Bestands-Endpunkt inkl. Kinder, setzt `RowVersion` als `OriginalValue` (Optimistic Concurrency), wendet Skalarwerte via `ApplyUpdate` an, löscht **alle** Bestands-Kinder und legt die übergebenen `Headers`/`QueryParameters` neu an — alles in einem `DbContext`/`SaveChangesAsync`.
- `EndpointsController.UpdateAsync` (Z.150–206) nutzt die Aggregat-Operation bereits, übergibt aber `Headers`/`QueryParameters` **nicht** im `Endpoint`-Objekt — danach folgt die redundante Doppelarbeit: manuelles `DeleteHeaderAsync`/`DeleteQueryParameterAsync` pro Bestands-Kind plus `AddHeaderAsync`/`AddQueryParameterAsync` pro Request-Kind.
- `ODataEndpointsController.Put` (Z.97–135) und `Patch` (Z.141–191) rufen `UpdateEndpointAsync` mit dem geladenen `existing`-Objekt auf — mitgelieferte `headers`/`queryparameters` im Request-Body werden still ignoriert (`ApplyPatch` kennt diese Felder nicht; in `Put` werden nur Skalare kopiert). Die EDM blendet `Headers`/`QueryParameters` aus (`ODataEdmModelBuilder`, Z.36–37).
- Import-Apply-Pfade (`SwaggerImportService.ApplyDiffAsync` Z.136–143, `ODataImportService.ApplyDiffAsync` Z.164–187) verwenden `UpdateEndpointAsync` bereits als Aggregat-Operation. `ImportDiffCalculator.MergeExistingIdentity` übernimmt bei geänderten Endpunkten die **Bestands**-Kinder (importierte Header werden bei ChangedEndpoints verworfen — Bestandsverhalten, nicht Teil dieser Anforderung).
- `EndpointPage.razor` persistiert Kinder doppelt: pro Panel-Änderung sofort via `AddHeaderAsync`/`DeleteHeaderAsync`/`AddQueryParameterAsync`/`DeleteQueryParameterAsync` (Z.444–474, verdrahtet über `OnHeaderAdded`/`OnHeaderRemoved`/`OnQueryParameterAdded`/`OnQueryParameterRemoved` in Z.74–83) **und** beim Speichern als Vollmenge via `UpdateEndpointRequest.Headers`/`QueryParameters` (Z.372–373 → `ApplicationApiClient.UpdateEndpointAsync` Z.315–339).
- REST-Vertrag `UpdateEndpointRequest` enthält `Headers`/`QueryParameters` als Vollmengen-Listen — Vollmengen-Ersetzung ist bereits Vertragsbestandteil.
- Einzel-Kind-Endpunkte `POST/DELETE /api/endpoints/headers` und `/api/endpoints/query-parameters` existieren weiterhin (für Neuanlage-Pfad in `EndpointPage.CreateEndpointWithDetailsAsync` genutzt, da `CreateEndpointRequest` keine Kinder enthält).
- Test-Ausgangszustand: 588 Tests, alle bestanden (`dotnet test --filter "FullyQualifiedName!~Playwright"`), Nachweis in [tests.md](inventory/tests.md).

## Details

- [Datenmodell und DTOs](inventory/models.md)
- [Logik und Abläufe](inventory/logic.md)
- [Interfaces](inventory/interfaces.md)
- [Tests](inventory/tests.md)
