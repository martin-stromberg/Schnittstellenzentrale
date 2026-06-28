# OData-Import aus CSDL-Metadaten

## Relevante Stellen

- `src/Schnittstellenzentrale.Infrastructure/Services/ODataImportService.cs`
- `src/Schnittstellenzentrale/OData/ODataEdmModelBuilder.cs`
- `src/Schnittstellenzentrale/OData/ODataAuthController.cs`
- `src/Schnittstellenzentrale/OData/ODataControllerBase.cs`
- `docs/help/api/odata-api.md`
- `src/Schnittstellenzentrale.Tests/Services/ODataImportTests.cs`
- `src/Schnittstellenzentrale.Tests/Services/EndpointScriptRunnerTests.cs`

## Aktueller Zustand

`ODataEdmModelBuilder` erzeugt ein EDM-Modell mit vier Entity-Sets und einer `Authenticate`-Action. Fuer die Entity-Sets werden Vocabulary-Annotationen gesetzt:

- `x-sz-auth-type`
- `x-sz-post-request-script`
- `x-sz-bearer-token`
- `x-sz-header-mode`

Die Authentifizierungs-Action bekommt ein eigenes Post-Skript, das den Token in `schnittstellenzentrale.authToken` ablegt.

`ODataImportService` liest Metadaten, erzeugt daraus Endpunkte und uebernimmt dieselben proprietaeren Annotationen:

- `x-sz-post-request-script` -> `PostRequestScript`
- `x-sz-bearer-token` -> Credential-Persistenz
- `x-sz-auth-type` -> Authentifizierungstyp

`ODataAuthController` stellt die Authenticate-Operation fuer OData getrennt bereit, und `ODataControllerBase` validiert Bearer-Tokens sowie den `X-Storage-Mode`-Header.

## Relevante Folgerung fuer die Aenderung

Wie bei Swagger liegt der relevante Aenderungspunkt nicht im Import-Mapping allein, sondern in der gemeinsamen Skript-API. Die OData-Authenticate-Action ist ein expliziter Sonderfall und darf keine rekursive Wiederholung durch neue Skriptlogik ausloesen.

## Testabdeckung

Vorhandene Tests decken bereits ab:

- Import von OData-Metadaten
- Erzeugung von Endpunkten und Gruppen
- Bearer-Token-Persistenz
- OData-Authentifizierungsfluss

Es fehlt aktuell eine direkte Pruefung, dass OData-generierte Post-Skripte die neue `sz.execute`-Boolean-Semantik und `sz.repeat()` korrekt nutzen koennen.
