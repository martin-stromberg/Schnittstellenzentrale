# REST-Import aus Swagger/OpenAPI

## Relevante Stellen

- `src/Schnittstellenzentrale.Infrastructure/Services/SwaggerImportService.cs`
- `src/Schnittstellenzentrale.Infrastructure/Helpers/SwaggerOperationHelper.cs`
- `src/Schnittstellenzentrale/Filters/SzExtensionsOperationFilter.cs`
- `src/Schnittstellenzentrale/Program.cs`
- `docs/help/api/api.md`
- `src/Schnittstellenzentrale.Tests/Services/SwaggerImportTests.cs`
- `src/Schnittstellenzentrale.Tests/Services/EndpointScriptRunnerTests.cs`

## Aktueller Zustand

`SzExtensionsOperationFilter` erweitert jede Swagger-Operation mit proprietaeren `x-sz-*`-Felder:

- `x-sz-bearer-token` fuer Bearer-Token-Platzhalter
- `x-sz-post-request-script` fuer Post-Skripte

Der Authenticate-Endpunkt `/authenticate` erhaelt ein eigenes Post-Skript, das den Token in `schnittstellenzentrale.authToken` ablegt. Alle anderen Operationen erhalten Token-Platzhalter und ein Refresh-Skript, das `X-New-Token` aus dem Response-Header uebernimmt.

`SwaggerOperationHelper.MapDocumentToEndpoints(...)` uebernimmt diese Erweiterungen beim Import:

- `x-sz-post-request-script` wird auf `PostRequestScript` gemappt
- `x-sz-bearer-token` setzt `AuthenticationType.BearerToken`
- Header-Parameter der Operation werden als Default-Header importiert

`SwaggerImportService` liest das OpenAPI-Dokument, berechnet den Diff und speichert Bearer-Tokens nach dem Import in den Credential Store.

## Relevante Folgerung fuer die Aenderung

Die Swagger-Seite der Anforderung wird nicht im Importer selbst ergaenzt, sondern ueber die generierte Skript-API und die bestehenden `x-sz-post-request-script`-Inhalte. Der Authenticate-Sonderfall ist im Filter bereits separat codiert und muss bei einer Wiederholungslogik weiterhin ausgeschlossen bleiben.

## Testabdeckung

Vorhandene Tests decken bereits ab:

- Mapping von OpenAPI-Operationen zu Endpunkten
- Persistenz von Bearer-Tokens beim Import
- Verarbeitung von Post-Skripten und Default-Headern
- Sonderfall des Authenticate-Endpunkts im Filter

Es fehlt aktuell eine explizite Absicherung dafuer, dass die generierten Skripte die neue Boolean-Semantik von `sz.execute` und die Wiederholung via `sz.repeat()` verwenden koennen.
