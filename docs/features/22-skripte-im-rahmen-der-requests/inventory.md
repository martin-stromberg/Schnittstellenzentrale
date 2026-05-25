# Bestandsaufnahme: Skripte und Variablen in der Swagger-Definition

Analysiert wurde der Bereich des Swagger-Imports sowie der Endpunkt-Ausführung, bezogen auf die Anforderung, beim Import einer Swagger/OpenAPI-Definition die Felder `PreRequestScript`, `PostRequestScript` und `AuthenticationType` (inkl. Bearer-Token-Wert) aus OpenAPI-Erweiterungsfeldern auszulesen und auf `Endpoint`-Instanzen zu übertragen.

## Zusammenfassung

- `Endpoint` besitzt bereits `PreRequestScript`, `PostRequestScript` und `AuthenticationType` — kein neues Feld im Datenmodell erforderlich, sofern der Bearer-Token-Wert weiterhin im Credential Manager abgelegt wird.
- Es gibt kein Feld `BearerTokenValue` oder `BearerTokenHint` im `Endpoint`-Modell; der Token-Wert wird ausschließlich über den Windows Credential Manager (`ICredentialService`) verwaltet.
- `SwaggerImportService.ImportAsync` belegt beim Import **nur** `Name`, `Method`, `RelativePath` und `ApplicationId`. OpenAPI-Erweiterungsfelder (`operation.Value.Extensions`) werden nicht ausgelesen.
- `SwaggerImportService` injiziert `ICredentialService` **nicht** — eine Ablage des Bearer-Token-Platzhalters im Credential Manager ist daher aktuell nicht möglich.
- `ImportDiffCalculator.HasChanged` vergleicht `Name`, `Body` und `AuthenticationType`, aber **nicht** `PreRequestScript` und `PostRequestScript`. Änderungen an Skripten beim Re-Import würden daher nicht erkannt.
- `ImportDiffCalculator.MergeExistingIdentity` überträgt `PreRequestScript` und `PostRequestScript` beim Merge **nicht** auf den aktualisierten Endpunkt.
- `EndpointExecutionService` führt Pre- und Post-Request-Skripte bereits vollständig aus; `ApplyAuthentication` liest den Bearer-Token über `ICredentialService` und löst `{{...}}`-Platzhalter auf.
- `ICredentialService` (inkl. `WindowsCredentialService`) und `CredentialTargetHelper` sind vollständig implementiert.
- In `SwaggerImportServiceTests` existieren keine Testfälle für Erweiterungsfelder oder Skript-/Auth-Belegung beim Import.

## Details

- [Datenmodell](inventory/models.md)
- [Logik](inventory/logic.md)
- [Enums](inventory/enums.md)
- [Interfaces](inventory/interfaces.md)
- [Tests](inventory/tests.md)
