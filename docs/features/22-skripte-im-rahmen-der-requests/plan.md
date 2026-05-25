# Umsetzungsplan: Skripte und Variablen in der Swagger-Definition

## Übersicht

Der `SwaggerImportService` wird erweitert, um beim Import einer Swagger/OpenAPI-Definition die OpenAPI-Erweiterungsfelder `x-sz-pre-request-script`, `x-sz-post-request-script` und `x-sz-bearer-token` auszulesen und auf die erzeugten `Endpoint`-Instanzen zu übertragen. `ImportDiffCalculator` wird angepasst, sodass Skripte und `AuthenticationType` beim Re-Import korrekt erkannt und übernommen werden. Der Bearer-Token-Wert wird per `ICredentialService.SavePassword` im Windows Credential Manager abgelegt, sodass keine Erweiterung des `Endpoint`-Datenmodells erforderlich ist; der Transport des Token-Werts zwischen `ImportAsync` und `ApplyDiffAsync` erfolgt über das neue Feld `ImportDiff.BearerTokens`. Alle Endpunkte werden durch die Erweiterungsfelder in der Swagger-Definition einheitlich gesteuert — es gibt keine hart codierten Sonderfälle im Import-Service.

## Programmabläufe

### Erstimport mit Erweiterungsfeldern

1. Die UI ruft `ISwaggerImportService.ImportAsync` mit dem `Application`-Objekt auf.
2. `SwaggerImportService.ImportAsync` lädt das Swagger-JSON per HTTP und parst es mit `OpenApiJsonReader`.
3. Für jede `OpenApiOperation` wird zusätzlich zu `Name`, `Method`, `RelativePath` und `ApplicationId` Folgendes ausgeführt:
   - Ist `x-sz-pre-request-script` in `operation.Value.Extensions` vorhanden, wird der Wert als `string` auf `Endpoint.PreRequestScript` gesetzt.
   - Ist `x-sz-post-request-script` vorhanden, wird der Wert auf `Endpoint.PostRequestScript` gesetzt.
   - Ist `x-sz-bearer-token` vorhanden, wird `Endpoint.AuthenticationType = AuthenticationType.BearerToken` gesetzt; der Token-Wert wird unter dem Schlüssel `"{Method}:{RelativePath}"` in einem lokalen Dictionary gespeichert.
4. `ImportDiffCalculator.Calculate` berechnet den Diff; neue Endpunkte mit Bearer-Token landen in `ImportDiff.NewEndpoints`, das lokale Token-Dictionary wird in `ImportDiff.BearerTokens` übertragen.
5. Die UI ruft `ISwaggerImportService.ApplyDiffAsync` mit dem berechneten `ImportDiff` auf.
6. `SwaggerImportService.ApplyDiffAsync` persistiert neue Endpunkte über `IEndpointRepository.AddEndpointAsync`.
7. Für jeden neuen Endpunkt mit `AuthenticationType.BearerToken` schlägt `ApplyDiffAsync` den Schlüssel `"{Method}:{RelativePath}"` in `ImportDiff.BearerTokens` nach und ruft `ICredentialService.SavePassword` mit dem Credential-Schlüssel (gebildet durch `CredentialTargetHelper.Build`) und dem importierten Token-Wert auf.

Beteiligte Klassen/Komponenten: `SwaggerImportService`, `ImportDiffCalculator`, `IEndpointRepository`, `ICredentialService`, `CredentialTargetHelper`, `Endpoint`, `ImportDiff`

### Re-Import (Diff) mit geänderten Skripten oder Bearer-Token

1. Die UI ruft `ISwaggerImportService.ImportAsync` erneut auf.
2. `SwaggerImportService.ImportAsync` liest Erweiterungsfelder wie beim Erstimport aus. Fehlen Erweiterungsfelder, bleiben `PreRequestScript`, `PostRequestScript` und `AuthenticationType` auf ihren jeweiligen Standardwerten (`null` bzw. `None`).
3. `ImportDiffCalculator.HasChanged` vergleicht den importierten Endpunkt mit dem bestehenden; dabei werden jetzt auch `PreRequestScript`, `PostRequestScript` und `AuthenticationType` einbezogen.
4. Endpunkte mit geänderten Skripten oder geändertem `AuthenticationType` landen in `ImportDiff.ChangedEndpoints`.
5. `ImportDiffCalculator.MergeExistingIdentity` überträgt zusätzlich `PreRequestScript` und `PostRequestScript` vom importierten auf den zusammengeführten Endpunkt. Fehlen die Erweiterungsfelder im Import, werden die Skripte auf `null` und `AuthenticationType` auf `None` zurückgesetzt.
6. `ApplyDiffAsync` persistiert geänderte Endpunkte über `IEndpointRepository.UpdateEndpointAsync`.
7. Für jeden geänderten Endpunkt mit `AuthenticationType.BearerToken` schlägt `ApplyDiffAsync` den Schlüssel `"{Method}:{RelativePath}"` in `ImportDiff.BearerTokens` nach und überschreibt den Credential-Manager-Eintrag via `ICredentialService.SavePassword`.

Beteiligte Klassen/Komponenten: `SwaggerImportService`, `ImportDiffCalculator`, `IEndpointRepository`, `ICredentialService`, `CredentialTargetHelper`, `Endpoint`, `ImportDiff`

## Neue Klassen

Keine.

## Änderungen an bestehenden Klassen

### `ImportDiff` (Datenmodellklasse)

- **Neue Eigenschaften:** `BearerTokens` (`IDictionary<string, string>`, Key: `"{Method}:{RelativePath}"`) — enthält die aus den Erweiterungsfeldern gelesenen Bearer-Token-Werte, damit `ApplyDiffAsync` nach dem Persistieren die korrekten Werte im Credential Manager ablegen kann. Der Schlüssel entspricht dem in `ImportDiffCalculator.BuildKey` verwendeten Format und ist vor der Persistierung stabil.

### `ImportDiffCalculator` (Klasse)

- **Geänderte Methoden:**
  - `HasChanged` — `PreRequestScript` und `PostRequestScript` werden in den Vergleich aufgenommen (zusätzlich zu den bereits verglichenen Feldern `Name`, `Body`, `AuthenticationType`).
  - `MergeExistingIdentity` — `PreRequestScript` und `PostRequestScript` werden vom importierten Endpunkt auf den zusammengeführten Endpunkt übertragen (bisher nicht übertragen). Enthält der importierte Endpunkt `null`-Werte, werden diese explizit übernommen und überschreiben damit eventuell vorhandene manuelle Werte.

### `SwaggerImportService` (Klasse)

- **Neue Methoden:** `ReadExtensionString(IDictionary<string, IOpenApiExtension> extensions, string key)` — liest einen OpenAPI-Erweiterungswert als `string?` aus einem Extensions-Dictionary; gibt `null` zurück, wenn der Schlüssel fehlt oder der Wert kein String ist. Private Hilfsmethode.
- **Geänderte Methoden:**
  - `ImportAsync` — liest nach dem Erzeugen jeder `Endpoint`-Instanz die Felder `x-sz-pre-request-script`, `x-sz-post-request-script` und `x-sz-bearer-token` aus den Erweiterungen, belegt `PreRequestScript`, `PostRequestScript` und `AuthenticationType` und trägt Bearer-Token-Werte in `ImportDiff.BearerTokens` ein. Alle Endpunkte werden einheitlich behandelt — kein Sonderfall nach Pfad oder Endpunktname.
  - `ApplyDiffAsync` — ruft nach dem Persistieren neuer oder geänderter Endpunkte mit `AuthenticationType.BearerToken` `ICredentialService.SavePassword` auf; schlägt den Token-Wert dazu in `ImportDiff.BearerTokens` nach.
- **Neue Abhängigkeit:** `ICredentialService` wird im Konstruktor injiziert.

## Datenbankmigrationen

Keine.

## Validierungsregeln

| Feld / Objekt | Regel | Fehlerfall |
|---|---|---|
| `x-sz-bearer-token` (Erweiterungswert) | Wenn `x-sz-bearer-token` vorhanden ist, muss der Wert ein nicht-leerer String sein | Leerer Wert wird ignoriert; `AuthenticationType` bleibt `None` und kein Credential-Eintrag wird angelegt |
| `x-sz-pre-request-script` / `x-sz-post-request-script` | Kein Syntax-Checking beim Import — Skripte werden unverändert übernommen | Syntaxfehler werden erst zur Laufzeit durch `EndpointScriptRunner` gemeldet |

## Konfigurationsänderungen

Keine.

## Seiteneffekte und Risiken

- **Re-Import überschreibt manuell gesetzte Skripte und AuthenticationType:** Durch das Überschreib-Verhalten beim Re-Import (Skripte auf `null`, `AuthenticationType` auf `None`, wenn Erweiterungsfelder fehlen) gehen manuell in der UI gesetzte Werte verloren. Dieses Verhalten ist explizit so vereinbart und entspricht dem bestehenden Verhalten für `Name` und `Body`.
- **`ImportDiffCalculator.HasChanged`:** Durch Hinzufügen von `PreRequestScript` und `PostRequestScript` in den Vergleich werden bei einem Re-Import Endpunkte, bei denen diese Felder manuell gesetzt wurden und in der Swagger-Definition fehlen, als „geändert" erkannt und in `ChangedEndpoints` aufgenommen. Dies führt zu einem `UpdateEndpointAsync`-Aufruf, der die Felder auf `null` setzt.
- **`ICredentialService.SavePassword` im Import:** Der Credential Manager wird während `ApplyDiffAsync` beschrieben — bisher geschah dies nur aus UI-Aktionen des Benutzers. Bei einem fehlgeschlagenen Credential-Speichervorgang (z. B. kein Zugriff auf den Credential Manager) muss der Fehler gefangen und geloggt werden, ohne den gesamten Import-Vorgang abzubrechen.
- **Bestehende Tests `Import_ChangedSwaggerOperation_ReturnsChangedInDiff` und ähnliche:** Die Tests verwenden `CreateService` ohne `ICredentialService`-Mock. Da `SwaggerImportService` eine neue Abhängigkeit erhält, muss `CreateService` angepasst werden. Inhaltlich bleiben die Tests korrekt, da fehlende Erweiterungsfelder zu `null`-Feldern führen.

## Umsetzungsreihenfolge

1. `ImportDiff` um `BearerTokens` (`IDictionary<string, string>`, Key: `"{Method}:{RelativePath}"`) erweitern.
2. `ImportDiffCalculator.HasChanged` um `PreRequestScript` und `PostRequestScript` erweitern.
3. `ImportDiffCalculator.MergeExistingIdentity` um Übertragung von `PreRequestScript` und `PostRequestScript` erweitern (einschließlich `null`-Werte).
4. `SwaggerImportService` um `ICredentialService`-Abhängigkeit im Konstruktor erweitern (DI-Registrierung prüfen — `ICredentialService` ist bereits im DI-Container vorhanden).
5. Private Hilfsmethode `ReadExtensionString` in `SwaggerImportService` anlegen.
6. `SwaggerImportService.ImportAsync` um Auslesen der Erweiterungsfelder `x-sz-pre-request-script`, `x-sz-post-request-script` und `x-sz-bearer-token` erweitern; alle Endpunkte einheitlich behandeln; Bearer-Token-Werte in `ImportDiff.BearerTokens` ablegen.
7. `SwaggerImportService.ApplyDiffAsync` um Aufruf von `ICredentialService.SavePassword` für Endpunkte mit `AuthenticationType.BearerToken` erweitern.
8. Hilfsmethode `CreateService` in `SwaggerImportServiceTests` um `ICredentialService`-Mock-Parameter erweitern; alle bestehenden Aufrufe anpassen.
9. Neue Testfälle in `SwaggerImportServiceTests` und `ImportDiffCalculatorTests` anlegen (siehe Abschnitt Tests).

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `Import_WithPostRequestScript_SetsPostRequestScriptOnEndpoint` | `SwaggerImportServiceTests` | Swagger-Definition mit `x-sz-post-request-script` → `NewEndpoints[0].PostRequestScript` enthält den erwarteten Wert |
| `Import_WithPreRequestScript_SetsPreRequestScriptOnEndpoint` | `SwaggerImportServiceTests` | Swagger-Definition mit `x-sz-pre-request-script` → `NewEndpoints[0].PreRequestScript` enthält den erwarteten Wert |
| `Import_WithBearerToken_SetsBearerTokenAuthTypeAndStoresBearerTokens` | `SwaggerImportServiceTests` | Swagger-Definition mit `x-sz-bearer-token` → `NewEndpoints[0].AuthenticationType == BearerToken` und `ImportDiff.BearerTokens` enthält den Token-Wert unter dem erwarteten Schlüssel |
| `Import_WithoutExtensions_LeavesScriptFieldsNull` | `SwaggerImportServiceTests` | Swagger-Definition ohne Erweiterungsfelder → `PreRequestScript`, `PostRequestScript` sind `null`, `AuthenticationType` ist `None` |
| `Import_ReImport_MissingExtensions_ResetsScriptsAndAuthType` | `SwaggerImportServiceTests` | Re-Import ohne Erweiterungsfelder bei vorhandenem Endpunkt → `ChangedEndpoints` enthält den Endpunkt mit `PreRequestScript = null`, `PostRequestScript = null`, `AuthenticationType = None` |
| `ApplyDiff_WithBearerToken_CallsSavePassword` | `SwaggerImportServiceTests` | `ApplyDiffAsync` mit einem neuen Endpunkt und Token-Eintrag in `BearerTokens` → `ICredentialService.SavePassword` wird mit dem erwarteten Schlüssel und Wert aufgerufen |
| `HasChanged_WhenPostRequestScriptDiffers_ReturnsTrue` | `ImportDiffCalculatorTests` (neue Testklasse) | `HasChanged` erkennt geändertes `PostRequestScript` als Änderung |
| `HasChanged_WhenPreRequestScriptDiffers_ReturnsTrue` | `ImportDiffCalculatorTests` | `HasChanged` erkennt geändertes `PreRequestScript` als Änderung |
| `MergeExistingIdentity_PreservesScripts` | `ImportDiffCalculatorTests` | `MergeExistingIdentity` überträgt `PreRequestScript` und `PostRequestScript` vom importierten Endpunkt |
| `MergeExistingIdentity_NullScripts_OverwritesExisting` | `ImportDiffCalculatorTests` | `MergeExistingIdentity` setzt `PreRequestScript` und `PostRequestScript` auf `null`, wenn der importierte Endpunkt `null`-Werte enthält |
| `CreateService(string swaggerJson, Mock<IEndpointRepository> repoMock, Mock<ICredentialService> credentialMock)` | `SwaggerImportServiceTests` | Erweiterte Hilfsmethode — stellt `SwaggerImportService` mit zusätzlichem `ICredentialService`-Mock bereit |

### Betroffene bestehende Tests

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| `CreateService` (Hilfsmethode in `SwaggerImportServiceTests`) | Muss um den `ICredentialService`-Mock-Parameter erweitert werden, da `SwaggerImportService` eine neue Abhängigkeit erhält. Alle bestehenden Aufrufe von `CreateService` müssen entsprechend angepasst werden. |

## Offene Punkte

Keine.
