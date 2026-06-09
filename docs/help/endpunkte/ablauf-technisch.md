# Endpunkte — Technischer Ablauf

## Übersicht

`EndpointPage` ist die zentrale Razor-Komponente für die Endpunkt-Bearbeitung. Sie verwaltet intern eine Liste von `RequestQueryParamsPanel.QueryParamEntry`-Objekten (`_queryParameters`), die sowohl Pfad-Platzhalter als auch reguläre Query-Parameter enthält. Die Unterscheidung erfolgt über das In-Memory-Flag `IsPathParameter`. Beim Laden, beim Verlassen des Pfadfelds und beim Speichern werden mehrere Hilfsmethoden koordiniert aufgerufen.

---

## Ablauf: Laden eines Endpunkts

### 1. Komponentenparameter empfangen

`EndpointPage.OnParametersSetAsync()` prüft, ob sich die `Endpoint.Id` geändert hat. Bei Änderung wird `LoadModelFromParameter()` aufgerufen.

Beteiligte Komponenten:
- `EndpointPage.OnParametersSetAsync()` — Einstiegspunkt für Parameteränderungen

### 2. Modell befüllen

`LoadModelFromParameter()` kopiert alle Felder von `Endpoint` in das lokale `_model` und befüllt `_queryParameters` aus `Endpoint.QueryParameters` — mit `IsPathParameter = false` als Startwert für alle Einträge.

Beteiligte Komponenten:
- `EndpointPage.LoadModelFromParameter()` — Modellinitialisierung
- `RequestQueryParamsPanel.QueryParamEntry` — In-Memory-Datenklasse mit `Key`, `Value`, `IsPathParameter`

### 3. Query-String extrahieren

`ExtractAndStripQueryString()` wird aufgerufen. Falls `_model.RelativePath` ein `?` enthält, wird der Query-String per `string.Split('?', 2)` abgetrennt. Jedes Schlüssel-Wert-Paar wird dekodiert (`Uri.UnescapeDataString`) und als neuer `QueryParamEntry` mit `IsPathParameter = false` in `_queryParameters` eingefügt — sofern kein Eintrag mit demselben Key bereits vorhanden ist. `_model.RelativePath` wird auf den Teil vor dem `?` gesetzt.

Beteiligte Komponenten:
- `EndpointPage.ExtractAndStripQueryString()` — Query-String-Extraktion und Pfadbereinigung

### 4. Pfad-Platzhalter synchronisieren

`SyncPathParameters()` wendet den Regex `\{([^}]+)\}` auf `_model.RelativePath` an. Für jeden gefundenen Platzhalternamen gilt:

- Kein vorhandener Eintrag mit `IsPathParameter = true` und diesem Key → neuer `QueryParamEntry` mit `IsPathParameter = true` wird am Listenanfang eingefügt.
- Bereits vorhandener Eintrag → bleibt unverändert (Value wird beibehalten).
- Vorhandene Einträge mit `IsPathParameter = true`, deren Key nicht mehr im Pfad vorkommt, werden entfernt.

Beteiligte Komponenten:
- `EndpointPage.SyncPathParameters()` — Platzhalter-Erkennung und Listensynchronisation

### 5. Anzeige aufbauen

Das Pfadfeld rendert `ResolveDisplayUrl()`. Diese Methode iteriert über `_queryParameters`:

- Einträge, deren Key als `{Key}` im Pfad vorkommt: Platzhalter wird durch `Uri.EscapeDataString(Value)` ersetzt.
- Übrige Einträge (ohne Platzhalter-Treffer): werden als `key=value`-Paare in einem Query-String gesammelt und mit `?` angehängt.

Beteiligte Komponenten:
- `EndpointPage.ResolveDisplayUrl()` — URL-Auflösung für die Anzeige
- `RequestQueryParamsPanel` — rendert die Parameterliste; Sortierung via `OrderByDescending(p => p.IsPathParameter)`

---

## Ablauf: Bearbeitung des Pfadfelds

### 1. Anwender verlässt das Pfadfeld

Der `@onblur`-Handler `OnPathBlur()` wird ausgelöst.

### 2. Verarbeitung in OnPathBlur

`OnPathBlur()` ruft sequenziell auf:

1. `ExtractAndStripQueryString()` — extrahiert ggf. neuen Query-String-Anteil
2. `SyncPathParameters()` — gleicht Platzhalter ab
3. `MarkDirty()` — markiert den Endpunkt als geändert und aktiviert Navigation Guards

Das Pfadfeld aktualisiert sich durch Re-Render mit dem neuen Rückgabewert von `ResolveDisplayUrl()`.

Beteiligte Komponenten:
- `EndpointPage.OnPathBlur()` — blur-Handler
- `EndpointPage.MarkDirty()` — setzt `_isDirty = true`, registriert `RegisterLocationChangingHandler` und aktiviert `window.onbeforeunload`

### 3. Änderung eines Query-Parameter-Werts

Sobald ein Name- oder Wert-Eingabefeld im `RequestQueryParamsPanel` verlassen wird (`@onchange`), ruft `OnFieldChanged` das `OnChanged`-Callback auf. `EndpointPage` empfängt dieses Event und rendert neu, wodurch das Pfadfeld die aktualisierte aufgelöste URL anzeigt.

Beteiligte Komponenten:
- `RequestQueryParamsPanel.OnFieldChanged()` — Event-Handler für Feldänderungen
- `EndpointPage` — empfängt `OnChanged` und rendert neu

---

## Ablauf: Speichern

`EndpointPage.SaveAsync()` baut `_model.QueryParameters` aus `_queryParameters` auf: sowohl Einträge mit `IsPathParameter = true` als auch `false` werden als gleichartige `EndpointQueryParameter`-Objekte übernommen (kein `IsPathParameter`-Feld in der Datenbank). Anschließend delegiert `SaveAsync()` an `PersistAsync()`.

`PersistAsync()` unterscheidet anhand von `_model.Id`:

- Neuer Endpunkt (`Id == 0`): `IApplicationApiClient.AddEndpointAsync(_model)` wird aufgerufen. Das zurückgegebene `Endpoint`-Objekt enthält die vergebene `Id` und den neuen `RowVersion`-Wert, die in `_model` übernommen werden.
- Bestehender Endpunkt: `IApplicationApiClient.UpdateEndpointAsync(_model)` wird aufgerufen. Der zurückgegebene `RowVersion`-Wert wird in `_model.RowVersion` übernommen.

`ForceSaveAsync()` liest vor dem Speichern den aktuellen `RowVersion`-Wert über `IApplicationApiClient.GetEndpointByIdAsync(_model.Id)` und aktualisiert `_model.RowVersion`, bevor `UpdateEndpointAsync` aufgerufen wird.

Beteiligte Komponenten:
- `EndpointPage.SaveAsync()` / `PersistAsync()` — Persistierungslogik
- `EndpointPage.ForceSaveAsync()` — liest `RowVersion` vor dem Speichern
- `IApplicationApiClient.AddEndpointAsync` / `UpdateEndpointAsync` / `GetEndpointByIdAsync` — HTTP-Transport
- `EndpointQueryParameter` — Datenbankmodell ohne `IsPathParameter`

---

## Ablauf: HTTP-Anfrage senden

### 1. Auslöser

`EndpointPage.SendRequestAsync()` lädt zunächst den aktuellen Endpunkt per `IApplicationApiClient.GetEndpointByIdAsync(_model.Id)`, um sicherzustellen, dass Header, Query-Parameter und `RowVersion` aktuell sind. Anschließend ruft es `ExecutionService.ExecuteAsync(refreshed)` mit dem frisch geladenen Objekt auf.

### 2. Rekursionsschutz-Initialisierung

`EndpointExecutionService.ExecuteAsync(endpoint)` legt ein leeres `Dictionary<int, int> callDepth` an und delegiert an die interne Überladung. Vor jeder Ausführung wird `callDepth[endpoint.Id]` geprüft: ist der Wert `>= 2`, wird sofort ein `EndpointExecutionResult { Success = false }` zurückgegeben.

Beteiligte Komponenten:
- `EndpointExecutionService.ExecuteAsync(endpoint, callDepth)` — Rekursionsschutz und Ausführungssteuerung

### 3. Pre-Request-Skript ausführen (optional)

Falls `endpoint.PreRequestScript` nicht leer ist, wird ein `ScriptContext` mit `ScriptRequestData` (Rohwerte vor Platzhalterauflösung), `IActiveEnvironmentService` und dem `ExecuteEndpoint`-Callback aufgebaut. `IEndpointScriptRunner.ExecuteAsync` wird aufgerufen.

- Bei `ScriptExecutionResult.Success == false`: `EndpointExecutionResult` mit `ErrorMessage` zurückgeben, Ausführung abbrechen — kein HTTP-Request.
- Bei Erfolg: Umgebungsvariablen ggf. durch `sz.environment.set()` bereits aktualisiert.

Beteiligte Komponenten:
- `EndpointExecutionService.BuildScriptContext()` — erstellt `ScriptContext`
- `EndpointScriptRunner.ExecuteAsync()` — führt das Skript im Jint-Interpreter aus
- `ScriptContext` / `ScriptRequestData` / `ScriptExecutionResult`

### 4. URL-Aufbau und HTTP-Anfrage senden

`EndpointExecutionService.BuildRequest()` löst `{{...}}`-Platzhalter in Basis-URL, relativem Pfad, Header-Namen/-Werten und Body via `IActiveEnvironmentService.ActiveVariables` auf — die vom Pre-Skript ggf. geänderten Werte sind bereits enthalten. Anschließend werden `{pfadparameter}` ersetzt und Query-Parameter angehängt.

Die endgültige URL wird zusammengesetzt:
```
baseUrl.TrimEnd('/') + "/" + resolvedPath.TrimStart('/')
```

Beteiligte Komponenten:
- `EndpointExecutionService.BuildRequest()` — URL-Zusammensetzung mit Platzhalter-Ersetzung
- `EndpointExecutionService.ExecuteWithAuthAsync()` / `ExecuteImpersonatedAsync()` — Authentifizierung und HTTP-Ausführung

### 5. Post-Request-Skript ausführen (optional)

Falls `endpoint.PostRequestScript` nicht leer ist, wird `ScriptResponseData` aus der HTTP-Antwort befüllt (Body, Headers) und der `ScriptContext` damit erweitert. `IEndpointScriptRunner.ExecuteAsync` wird aufgerufen.

- Bei `ScriptExecutionResult.Success == false`: `EndpointExecutionResult.ErrorMessage` wird gesetzt oder ergänzt; das HTTP-Ergebnis bleibt erhalten.

Beteiligte Komponenten:
- `EndpointScriptRunner.ExecuteAsync()` — führt das Post-Skript aus
- `ScriptResponseData` — Snapshot der HTTP-Antwort

### 6. Ergebnis zurückgeben

`callDepth[endpoint.Id]` wird dekrementiert; das `EndpointExecutionResult` wird zurückgegeben.

---

## Ablauf: `sz.environment.set()` im Skript

### Mit aktiver Systemumgebung (Persistierungsfall)

1. Das Skript ruft `sz.environment.set(name, value)` auf.
2. `EndpointScriptRunner.BuildEnvironmentObject` liest `context.EnvironmentService.ActiveEnvironment` — Ergebnis ist nicht `null`.
3. Aus `context.EnvironmentService.ActiveVariables` wird ein aktualisiertes Dictionary aufgebaut (`updatedVariables[name] = value`).
4. Beim Neuaufbau der `EnvironmentVariable`-Objekte werden `Id` und `IsValueMasked` aus `ActiveEnvironment.Variables` für Variablen mit übereinstimmendem Namen übernommen. Die Maskierungsflagge geht damit nicht verloren.
5. `context.EnvironmentService.SetActiveEnvironment(updatedEnv)` aktualisiert den In-Memory-Zustand.
6. Da `activeEnv != null` und `context.EnvironmentRepository != null`, wird `PersistVariable` aufgerufen:
   - `context.EnvironmentRepository.UpdateVariableAsync(activeEnv.Id, name, value)` wird via `Task.Run(...).GetAwaiter().GetResult()` blockierend ausgeführt.
   - Anschließend wird `context.SignalRNotificationService.NotifyEnvironmentChangedAsync()` aufgerufen (falls gesetzt), damit verbundene Clients die Änderung sofort erhalten.
7. Wirft einer der Aufrufe eine Exception, wird diese propagiert; `ExecuteAsync` gibt ein `ScriptExecutionResult { Success = false }` zurück.

Beteiligte Komponenten:
- `EndpointScriptRunner.BuildEnvironmentObject` — Lambda-Implementierung mit Persistierungslogik
- `EndpointScriptRunner.PersistVariable` — blockierender Aufruf von Repository und SignalR
- `ScriptContext.EnvironmentRepository` (`ISystemEnvironmentRepository`) — Zugriff auf `UpdateVariableAsync`
- `ScriptContext.SignalRNotificationService` (`ISignalRNotificationService`) — Benachrichtigung verbundener Clients
- `IActiveEnvironmentService.SetActiveEnvironment` — In-Memory-Aktualisierung

### Ohne aktive Systemumgebung (unverändertes Verhalten)

1. Das Skript ruft `sz.environment.set(name, value)` auf.
2. `context.EnvironmentService.ActiveEnvironment` ist `null`.
3. Der In-Memory-Zustand wird via `SetActiveEnvironment` aktualisiert.
4. Da `activeEnv == null`, werden weder `UpdateVariableAsync` noch `NotifyEnvironmentChangedAsync` aufgerufen.

---

## Ablauf: Header und Query-Parameter verwalten

`EndpointPage` verwaltet Header und Query-Parameter über atomare API-Routen:

- **Header hinzufügen** (`AddHeaderAsync`): `IApplicationApiClient.AddHeaderAsync(header)` → `POST /api/endpoints/headers` → gibt `EndpointHeader` mit vergebener `Id` zurück; wird in `_model.Headers` eingefügt.
- **Header löschen** (`DeleteHeaderAsync`): `IApplicationApiClient.DeleteHeaderAsync(id)` → `DELETE /api/endpoints/headers/{id}` → Eintrag wird aus `_model.Headers` entfernt.
- **Query-Parameter hinzufügen** (`AddQueryParameterAsync`): `IApplicationApiClient.AddQueryParameterAsync(parameter)` → `POST /api/endpoints/query-parameters` → gibt `EndpointQueryParameter` zurück.
- **Query-Parameter löschen** (`DeleteQueryParameterAsync`): `IApplicationApiClient.DeleteQueryParameterAsync(id)` → `DELETE /api/endpoints/query-parameters/{id}`.

Beteiligte Komponenten:
- `EndpointPage.AddHeaderAsync()` / `DeleteHeaderAsync()` / `AddQueryParameterAsync()` / `DeleteQueryParameterAsync()`
- `IApplicationApiClient` — HTTP-Transport
- `RequestHeadersPanel` / `RequestQueryParamsPanel` — UI-Komponenten, die die jeweiligen `OnAdd`/`OnDelete`-Callbacks auslösen

---

## Ablauf: `sz.execute()` im Skript

1. Das Skript ruft `sz.execute(name)` auf.
2. `EndpointScriptRunner` ruft den `ExecuteEndpoint`-Callback im `ScriptContext` auf via `Task.Run(...).GetAwaiter().GetResult()`.
3. `EndpointExecutionService` sucht via `IEndpointRepository.GetEndpointByNameAsync(applicationId, name)` nach dem Endpunkt. Bei 0 oder mehreren Treffern wird ein Fehler zurückgegeben.
4. `ExecuteAsync` wird rekursiv mit dem bestehenden `callDepth` aufgerufen. Der Rekursionsschutz greift bei `callDepth[id] >= 2`.
5. Das Ergebnis wird als JavaScript-Objekt zurückgegeben: `{ success, statusCode, responseBody, errorMessage }`.

Beteiligte Komponenten:
- `EndpointScriptRunner` — synchroner Callback-Aufruf
- `EndpointExecutionService.ExecuteEndpoint`-Callback — Endpunkt-Lookup und rekursiver Aufruf
- `IEndpointRepository.GetEndpointByNameAsync()` — Namens-Lookup

---

## Ablauf: Swagger-Import mit Erweiterungsfeldern

### 1. Auslöser

Die UI ruft `ISwaggerImportService.ImportAsync(application)` auf.

### 2. Swagger-Definition laden und parsen

`SwaggerImportService.ImportAsync` lädt das Swagger-JSON per HTTP über `IHttpClientFactory` und parst es mit `OpenApiJsonReader`. Treten Parsing-Fehler auf, wird ein `ImportDiff { ErrorMessage = ... }` zurückgegeben.

Beteiligte Komponenten:
- `SwaggerImportService.ImportAsync` — HTTP-Abruf und Parsing
- `OpenApiJsonReader` — Parsing der OpenAPI-Definition

### 3. Endpunkte mit Erweiterungsfeldern erzeugen

Für jede `OpenApiOperation` in `document.Paths` wird ein `Endpoint` erzeugt. Zusätzlich zu `Name`, `Method`, `RelativePath` und `ApplicationId` werden folgende Felder gesetzt:

- `x-sz-pre-request-script` → `Endpoint.PreRequestScript`
- `x-sz-post-request-script` → `Endpoint.PostRequestScript`
- `x-sz-bearer-token` → `Endpoint.AuthenticationType = BearerToken`; Token-Wert wird unter dem Schlüssel `"{Method}:{RelativePath}"` in einem lokalen Dictionary gesammelt.

Fehlende Erweiterungsfelder belassen die Felder auf `null` bzw. `None`.

Beteiligte Komponenten:
- `SwaggerImportService.ReadExtensionString(extensions, key)` — liest einen OpenAPI-Erweiterungswert als `string?`; gibt `null` zurück, wenn der Schlüssel fehlt oder der Wert kein String ist
- `ImportDiff.BearerTokens` (`IDictionary<string, string>`) — transportiert die Token-Werte zwischen `ImportAsync` und `ApplyDiffAsync`

### 4. Diff berechnen

`ImportDiffCalculator.Calculate(existing, imported)` vergleicht bestehende und importierte Endpunkte. `ImportDiffCalculator.HasChanged` berücksichtigt jetzt auch `PreRequestScript` und `PostRequestScript`. `ImportDiffCalculator.MergeExistingIdentity` überträgt diese Felder vom importierten Endpunkt auf den zusammengeführten Eintrag (einschließlich `null`-Werte).

Beteiligte Komponenten:
- `ImportDiffCalculator.Calculate` — Diff-Berechnung
- `ImportDiffCalculator.HasChanged` — Änderungserkennung inkl. Skriptfelder
- `ImportDiffCalculator.MergeExistingIdentity` — Zusammenführung mit bestehender Identität

Das lokale Bearer-Token-Dictionary wird als `ImportDiff.BearerTokens` in das zurückgegebene `ImportDiff` übernommen.

### 5. Diff anwenden

Die UI ruft `ISwaggerImportService.ApplyDiffAsync(diff)` auf. Für neue und geänderte Endpunkte werden zunächst die Datenbankoperationen ausgeführt (`IEndpointRepository.AddEndpointAsync` / `UpdateEndpointAsync`). Anschließend wird `SaveBearerTokenIfPresent` aufgerufen:

- `endpoint.AuthenticationType == BearerToken` → Schlüssel `"{Method}:{RelativePath}"` in `ImportDiff.BearerTokens` nachschlagen → `ICredentialService.SavePassword(credentialTarget, "", tokenValue)` aufrufen.
- Fehler beim Credential-Zugriff werden per `_logger.LogWarning` geloggt und unterbrechen den Import nicht.
- Der `credentialTarget` wird durch `CredentialTargetHelper.Build(applicationId, AuthenticationType.BearerToken)` erzeugt.

Beteiligte Komponenten:
- `SwaggerImportService.ApplyDiffAsync` — Persistierung und Credential-Ablage
- `SwaggerImportService.SaveBearerTokenIfPresent` — private Hilfsmethode für Credential-Speicherung
- `IEndpointRepository.AddEndpointAsync` / `UpdateEndpointAsync` — Datenbankoperationen
- `ICredentialService.SavePassword` — Ablage im Windows Credential Manager
- `CredentialTargetHelper.Build` — Schlüsselbildung für den Credential Manager

---

## Diagramm

```mermaid
flowchart TD
    A[SendRequestAsync] --> B[ExecuteAsync - callDepth prüfen]
    B --> C{callDepth >= 2?}
    C -- Ja --> D[Fehler: Rekursionsschutz]
    C -- Nein --> E{PreRequestScript?}
    E -- Ja --> F[ScriptContext aufbauen]
    F --> G[EndpointScriptRunner.ExecuteAsync Pre]
    G --> H{Success?}
    H -- Nein --> I[Fehler zurückgeben - kein HTTP]
    H -- Ja --> J[BuildRequest mit aufgelösten Variablen]
    E -- Nein --> J
    J --> K[HTTP-Request senden]
    K --> L{PostRequestScript?}
    L -- Ja --> M[ScriptResponseData befüllen]
    M --> N[EndpointScriptRunner.ExecuteAsync Post]
    N --> O{Success?}
    O -- Nein --> P[ErrorMessage ergänzen]
    P --> Q[EndpointExecutionResult zurückgeben]
    O -- Ja --> Q
    L -- Nein --> Q

    G2[sz.execute in Skript] --> R[ExecuteEndpoint-Callback]
    R --> S[GetEndpointByNameAsync]
    S --> T{Eindeutiger Treffer?}
    T -- Nein --> U[Fehler]
    T -- Ja --> B

    G3[sz.environment.set in Skript] --> V{ActiveEnvironment != null?}
    V -- Nein --> W[Nur In-Memory aktualisieren]
    V -- Ja --> X[SetActiveEnvironment - In-Memory]
    X --> Y[UpdateVariableAsync - Datenbank]
    Y --> Z[NotifyEnvironmentChangedAsync - SignalR]
    Y -- Exception --> AA[ScriptExecutionResult - Success=false]
    Z -- Exception --> AA
```

---

## Ablauf: OData-Import

### 1. Auslöser

Der Anwender klickt auf **OData-Import** in `ApplicationContentView`. `ApplicationContentView.OpenODataImportAsync()` ruft `IODataImportService.ImportAsync(application)` auf.

### 2. Metadatenabruf

`ODataImportService.ImportAsync` prüft zunächst `application.InterfaceUrl`: ist der Wert leer, wird sofort eine leere `ImportDiff` zurückgegeben. Andernfalls wird das CSDL-Dokument per `IHttpClientFactory.CreateClient().GetStringAsync(interfaceUrl)` abgerufen.

Beteiligte Komponenten:
- `ODataImportService.ImportAsync` — HTTP-Abruf und Steuerungslogik
- `IHttpClientFactory` — HTTP-Client-Bereitstellung

### 3. CSDL parsen

Das abgerufene XML wird mit `CsdlReader.Parse(XmlReader.Create(new StringReader(xmlContent)))` aus `Microsoft.OData.Edm.Csdl` in ein `IEdmModel` überführt.

Beteiligte Komponenten:
- `CsdlReader.Parse` (`Microsoft.OData.Edm.Csdl`) — CSDL-Parsing

### 4. Annotationen parsen

Bevor Endpunkte erzeugt werden, liest `ODataImportService` die proprietären `x-sz-*`-Annotationen aus dem CSDL-XML. Die Annotation-Dictionaries werden separat für Entity-Sets und Operationen aufgebaut; sowohl Inline-Annotationen (direkt im Element) als auch externe `<Annotations Target="...">` -Blöcke werden berücksichtigt.

Beteiligte Komponenten:
- `ODataImportService.ParseEntitySetAnnotations` / `ParseOperationAnnotations` — Annotation-Extraktion per XDocument
- `ODataImportService.MergeAnnotation` — fügt erkannte `x-sz-*`-Annotationen in das Dictionary ein

### 5. Endpunkte ableiten

Für jeden `IEdmEntitySet` in `model.EntityContainer.EntitySets()` werden fünf `Endpoint`-Objekte erzeugt:

| Eigenschaft | Wert |
|-------------|------|
| `Name` | `"GET {EntitySet.Name}"`, `"POST {EntitySet.Name}"`, `"PUT {EntitySet.Name}"`, `"PATCH {EntitySet.Name}"`, `"DELETE {EntitySet.Name}"` |
| `Method` | `GET`, `POST`, `PUT`, `PATCH`, `DELETE` |
| `RelativePath` | Relativ zur `Application.BaseUrl` (z. B. `odatav4/Products` oder `Products({key})` für PUT/PATCH/DELETE) |
| `ApplicationId` | `application.Id` |
| `AuthenticationType` | Aus `x-sz-auth-type`-Annotation oder Standard (BearerToken wenn bestehender Token vorhanden, sonst None) |
| `PostRequestScript` | Aus `x-sz-post-request-script`-Annotation, sonst `null` |
| `Headers` | Aus `x-sz-header-{name}`-Annotationen, sonst leere Liste |

Für jede `IEdmOperation` (aus `model.SchemaElements.OfType<IEdmOperation>()`) wird ein weiterer `Endpoint` erzeugt: `IEdmAction` ergibt `POST`, `IEdmFunction` ergibt `GET`.

Bearer-Tokens aus `x-sz-bearer-token`-Annotationen werden in einem lokalen `bearerTokens`-Dictionary gesammelt (Schlüssel: `EndpointKeyHelper.BuildKey(endpoint)`).

Beteiligte Komponenten:
- `ODataImportService.ImportAsync` — Endpunkt-Ableitung aus dem EDM-Modell
- `ODataImportService.AddEndpoint` / `ResolveAuthType` / `BuildRelativePath` — Hilfsmethoden für Pfad- und Authentifizierungsberechnung

### 6. Diff berechnen

`ImportDiffCalculator.Calculate(existingEndpoints, importedEndpoints)` wird identisch zum Swagger-Import-Pfad aufgerufen. Das `bearerTokens`-Dictionary wird per `.WithBearerTokens(bearerTokens)` in das zurückgegebene `ImportDiff` übernommen.

Beteiligte Komponenten:
- `ImportDiffCalculator.Calculate` — Diff-Berechnung (wiederverwendet)
- `IEndpointRepository.GetEndpointsAsync(applicationId)` — lädt bestehende Endpunkte

### 7. Dialog anzeigen

`ApplicationContentView` empfängt die `ImportDiff`. Enthält `ImportDiff.ErrorMessage` einen Wert, wird der Dialog nicht geöffnet und die Fehlermeldung in `_errorMessage` gesetzt; ein inline-Fehler-Alert erscheint im `sz-hero-right`-Bereich. Andernfalls wird `_showODataImport = true` gesetzt, was `ODataImportDialog` einblendet.

`ODataImportDialog` delegiert an `ImportDialog` (wiederverwendet) mit dem Titel `L["ODataImportDialog_Title"]` (DE: „OData-Import-Vorschau").

Beteiligte Komponenten:
- `ApplicationContentView.OpenODataImportAsync()` — Fehlerprüfung und Dialog-Steuerung
- `ODataImportDialog` — Wrapper-Komponente
- `ImportDialog` — generische Dialog-Komponente (wiederverwendet)

### 8. Diff anwenden

`ODataImportDialog.ApplyAsync` ruft `IODataImportService.ApplyDiffAsync(diff)` auf.

`ODataImportService.ApplyDiffAsync` iteriert über alle betroffenen `applicationId`-Werte, lädt die vorhandenen Endpunktgruppen und baut ein `groupLookup`-Dictionary auf. Anschließend werden die Endpunkte verarbeitet:

1. **Neue Endpunkte** (`diff.NewEndpoints`): Gruppe nach Entity-Set-Namen suchen oder anlegen; `EndpointGroupId` setzen; `IEndpointRepository.AddEndpointAsync(endpoint)` aufrufen.
2. **Geänderte Endpunkte** (`diff.ChangedEndpoints`): Gruppe wie oben; `IEndpointRepository.UpdateEndpointAsync(endpoint)` aufrufen.
3. **Entfernte Endpunkte** (`diff.RemovedEndpoints`): `IEndpointRepository.DeleteEndpointAsync(endpoint.Id)` aufrufen.
4. **Bearer-Tokens** (`SaveBearerTokenOnce`): Für jeden Endpunkt mit `AuthenticationType = BearerToken` und vorhandenem Token-Wert in `diff.BearerTokens` wird `ICredentialService.SavePassword` aufgerufen (maximal einmal pro Anwendung).

Beteiligte Komponenten:
- `ODataImportService.ApplyDiffAsync` — Persistierung über `IEndpointRepository`
- `ODataImportService.SaveBearerTokenOnce` — Bearer-Token-Persistierung im Credential Manager
- `ODataImportService.ExtractEntitySetName` — Ableitung des Gruppennamens aus dem Endpunkt-Namen

### Diagramm: OData-Import

```mermaid
flowchart TD
    A[OpenODataImport] --> B{InterfaceUrl leer?}
    B -- Ja --> C[Leere ImportDiff zurückgeben]
    B -- Nein --> D[HTTP: GetStringAsync InterfaceUrl]
    D --> E{HTTP-Fehler?}
    E -- Ja --> F[ImportDiff mit ErrorMessage]
    E -- Nein --> G[CsdlReader.Parse XmlReader]
    G --> H{XML/CSDL-Fehler?}
    H -- Ja --> F
    H -- Nein --> I[ParseEntitySetAnnotations + ParseOperationAnnotations]
    I --> J[Für jeden EntitySet: 5 Endpunkte erzeugen GET POST PUT PATCH DELETE]
    J --> K[Für jede IEdmOperation: Endpunkt erzeugen]
    K --> L[GetEndpointsAsync - Bestand laden]
    L --> M[ImportDiffCalculator.Calculate + WithBearerTokens]
    M --> N{ErrorMessage gesetzt?}
    N -- Ja --> O[Fehlermeldung in ApplicationContentView]
    N -- Nein --> P[ODataImportDialog öffnen]
    P --> Q{Anwender: Übernehmen oder Abbrechen?}
    Q -- Abbrechen --> R[Dialog schließen]
    Q -- Übernehmen --> S[ApplyDiffAsync]
    S --> T[Gruppe pro EntitySet suchen oder anlegen]
    T --> U[AddEndpointAsync für NewEndpoints]
    U --> V[UpdateEndpointAsync für ChangedEndpoints]
    V --> W[DeleteEndpointAsync für RemovedEndpoints]
    W --> X[SaveBearerTokenOnce - Credential Manager]
    X --> R
```

---

## Diagramm: Swagger-Import

```mermaid
flowchart TD
    A[ImportAsync] --> B{HTTP-Fehler?}
    B -- Ja --> C[ImportDiff mit ErrorMessage]
    B -- Nein --> D[OpenApiJsonReader.ReadAsync]
    D --> E{Parsing-Fehler?}
    E -- Ja --> C
    E -- Nein --> F[Für jede OpenApiOperation]
    F --> G[Endpoint erzeugen]
    G --> H[ReadExtensionString x-sz-pre-request-script]
    H --> I[ReadExtensionString x-sz-post-request-script]
    I --> J{x-sz-bearer-token vorhanden?}
    J -- Ja --> K[AuthenticationType = BearerToken; bearerTokens-Dict füllen]
    J -- Nein --> L[ImportDiffCalculator.Calculate]
    K --> L
    L --> M[ImportDiff mit BearerTokens zurückgeben]
    M --> N[ApplyDiffAsync]
    N --> O[AddEndpointAsync / UpdateEndpointAsync]
    O --> P{AuthenticationType = BearerToken?}
    P -- Ja --> Q[SaveBearerTokenIfPresent]
    Q --> R{Credential-Fehler?}
    R -- Ja --> S[LogWarning - Import fortsetzen]
    R -- Nein --> T[ICredentialService.SavePassword]
    P -- Nein --> U[Nächster Endpunkt]
    T --> U
    S --> U
```

---

## Fehlerbehandlung

- `PersistAsync()` fängt `DbUpdateConcurrencyException` und zeigt den `ConcurrencyWarningDialog`.
- `ExecuteAsync()` fängt allgemeine Ausnahmen und gibt ein `EndpointExecutionResult` mit `Success = false` und `ErrorMessage` zurück.
- Pre-Skript-Fehler (Syntaxfehler, Runtime-Exception, Timeout) verhindern den HTTP-Request; `ErrorMessage` enthält die Fehlerbeschreibung.
- Post-Skript-Fehler hängen die Fehlermeldung an ein ansonsten vollständiges `EndpointExecutionResult` an.
- `EndpointScriptRunner` begrenzt die Skriptlaufzeit auf `ScriptTimeoutMs = 5000 ms` und den Arbeitsspeicher auf 4 MB.
- `sz.environment.set()` propagiert Exceptions aus `UpdateVariableAsync` oder `NotifyEnvironmentChangedAsync` ohne Behandlung; das Skript schlägt damit fehl und das `EndpointExecutionResult` enthält die Fehlermeldung.
- Einträge mit leerem `Key` werden in `BuildRequest()` und in `ResolveDisplayUrl()` übersprungen.
- Import-Fehler beim Laden oder Parsen des Swagger-JSONs erzeugen ein `ImportDiff { ErrorMessage = ... }` und verhindern jede weitere Verarbeitung.
- Fehler beim Schreiben in den Windows Credential Manager werden per `_logger.LogWarning` protokolliert; die restlichen Endpunkte des Imports werden trotzdem persistiert.
- OData-Import: `HttpRequestException` beim Metadatenabruf → `ImportDiff { ErrorMessage = "HTTP-Fehler beim Abruf der Metadaten: ..." }`. `XmlException` beim Parsing → `ImportDiff { ErrorMessage = "Ungültiges XML in Metadaten: ..." }`. Sonstige Ausnahmen beim Parsing → `ImportDiff { ErrorMessage = "Fehler beim Parsen der Metadaten: ..." }`. In allen Fehlerfall-Varianten wird der Fehler per `_logger.LogWarning` protokolliert.
