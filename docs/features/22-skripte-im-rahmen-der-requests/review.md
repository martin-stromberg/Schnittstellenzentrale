# Plan-Review

## Ergebnis

**Status:** Vollständig umgesetzt

## Umgesetzte Planelemente

### `ImportDiff` (Datenmodellklasse)

- [x] Feld `BearerTokens` (`IDictionary<string, string>`) in `ImportDiff` — vorhanden

### `ImportDiffCalculator` (Klasse)

- [x] Methode `HasChanged` in `ImportDiffCalculator` — erweitert um `PreRequestScript` und `PostRequestScript` im Vergleich
- [x] Methode `MergeExistingIdentity` in `ImportDiffCalculator` — erweitert um Übertragung von `PreRequestScript` und `PostRequestScript` (inkl. `null`-Werte)

### `SwaggerImportService` (Klasse)

- [x] Abhängigkeit `ICredentialService` im Konstruktor von `SwaggerImportService` — injiziert
- [x] Private Hilfsmethode `ReadExtensionString` in `SwaggerImportService` — angelegt
- [x] Methode `ImportAsync` in `SwaggerImportService` — erweitert um Auslesen von `x-sz-pre-request-script`, `x-sz-post-request-script` und `x-sz-bearer-token`; Bearer-Token-Werte werden in `ImportDiff.BearerTokens` abgelegt; alle Endpunkte einheitlich behandelt
- [x] Methode `ApplyDiffAsync` in `SwaggerImportService` — erweitert um Aufruf von `ICredentialService.SavePassword` für Endpunkte mit `AuthenticationType.BearerToken` (via private Hilfsmethode `SaveBearerTokenIfPresent`)

### Tests

- [x] `CreateService(string, Mock<IEndpointRepository>, Mock<ICredentialService>)` in `SwaggerImportServiceTests` — erweitert um `ICredentialService`-Mock-Parameter (optional, mit Fallback auf neuen Mock)
- [x] `Import_WithPostRequestScript_SetsPostRequestScriptOnEndpoint` in `SwaggerImportServiceTests` — vorhanden
- [x] `Import_WithPreRequestScript_SetsPreRequestScriptOnEndpoint` in `SwaggerImportServiceTests` — vorhanden
- [x] `Import_WithBearerToken_SetsBearerTokenAuthTypeAndStoresBearerTokens` in `SwaggerImportServiceTests` — vorhanden
- [x] `Import_WithoutExtensions_LeavesScriptFieldsNull` in `SwaggerImportServiceTests` — vorhanden
- [x] `Import_ReImport_MissingExtensions_ResetsScriptsAndAuthType` in `SwaggerImportServiceTests` — vorhanden
- [x] `ApplyDiff_WithBearerToken_CallsSavePassword` in `SwaggerImportServiceTests` — vorhanden
- [x] `HasChanged_WhenPostRequestScriptDiffers_ReturnsTrue` in `ImportDiffCalculatorTests` — vorhanden
- [x] `HasChanged_WhenPreRequestScriptDiffers_ReturnsTrue` in `ImportDiffCalculatorTests` — vorhanden
- [x] `MergeExistingIdentity_PreservesScripts` in `ImportDiffCalculatorTests` — vorhanden
- [x] `MergeExistingIdentity_NullScripts_OverwritesExisting` in `ImportDiffCalculatorTests` — vorhanden

## Offene Aufgaben

Keine.

## Hinweise

- `ApplyDiffAsync` delegiert das Speichern des Bearer-Tokens an eine private Hilfsmethode `SaveBearerTokenIfPresent`, die nicht im Plan erwähnt ist, aber den geplanten Ablauf vollständig umsetzt. Fehler beim Credential-Manager-Zugriff werden abgefangen und geloggt, wie im Risikoabschnitt des Plans gefordert.
- `ImportDiffCalculatorTests` ist eine neue Testklasse (im Plan als neu klassifiziert); alle vier darin geplanten Testmethoden sind vorhanden.
