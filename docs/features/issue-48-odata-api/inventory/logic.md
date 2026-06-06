# Logik

## `ODataImportService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/ODataImportService.cs`

Implementiert `IODataImportService`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `ImportAsync(Application application)` | `public` | HTTP-Abruf des CSDL-Dokuments via `IHttpClientFactory`, Parsing mit `CsdlReader.Parse(XmlReader)`, Abbildung auf `Endpoint`-Objekte, Diff-Berechnung via `ImportDiffCalculator.Calculate` |
| `ApplyDiffAsync(ImportDiff diff)` | `public` | Iteriert über `NewEndpoints`, `ChangedEndpoints`, `RemovedEndpoints` und delegiert an `IEndpointRepository` |

**Abhängigkeiten:** `IHttpClientFactory`, `IEndpointRepository`, `ILogger<ODataImportService>`

**Endpunkt-Abbildung:**
- Für jedes `IEdmEntitySet` in `model.EntityContainer.EntitySets()`: je ein `GET`- und `POST`-Endpunkt mit `RelativePath = entitySet.Name`
- Für jede `IEdmOperation` in `model.SchemaElements.OfType<IEdmOperation>()`: `POST` bei `IEdmAction`, `GET` bei `IEdmFunction`; `RelativePath = operation.Name`

**Fehlerbehandlung:**
- `HttpRequestException` → `ImportDiff { ErrorMessage = "HTTP-Fehler …" }`
- `XmlException` → `ImportDiff { ErrorMessage = "Ungültiges XML …" }`
- Beliebige `Exception` → `ImportDiff { ErrorMessage = "Fehler beim Parsen …" }`
- `InterfaceUrl` ist `null` oder leer → leere `ImportDiff` ohne Fehler

**Hinweis:** `ApplyDiffAsync` ruft weder `EndpointGroupHelper.ResolveGroupIdAsync` noch `_credentialService.SavePassword` auf — im Gegensatz zu `SwaggerImportService.ApplyDiffAsync`. Endpunkte werden ohne Ordnerzuweisung und ohne Bearer-Token-Persistierung gespeichert.

---

## `ImportDiffCalculator`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/ImportDiffCalculator.cs`

Interne statische Hilfsklasse, die von `ODataImportService` und `SwaggerImportService` aufgerufen wird.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `Calculate(IList<Endpoint> existing, IList<Endpoint> imported)` | `internal static` | Berechnet `NewEndpoints`, `ChangedEndpoints`, `RemovedEndpoints` anhand des Schlüssels `{Method}:{RelativePath}` |
| `TryBuildDictionary(...)` | `private static` | Erstellt ein Dictionary Schlüssel → Endpunkt; erkennt Duplikate |
| `HasChanged(Endpoint existing, Endpoint imported)` | `private static` | Vergleicht `Name`, `Body`, `AuthenticationType`, `PreRequestScript`, `PostRequestScript` |
| `MergeExistingIdentity(Endpoint existing, Endpoint imported)` | `private static` | Kombiniert Identity-Felder (`Id`, `RowVersion`, `EndpointGroupId`, `Headers`, `QueryParameters`) aus dem Bestand mit Inhaltsfeldern aus dem Import |
| `BuildKey(Endpoint endpoint)` | `private static` | Delegiert an `EndpointKeyHelper.BuildKey` |

Aufgerufen von: `ODataImportService.ImportAsync`, `SwaggerImportService.ImportAsync`

---

## `ODataEdmModelBuilder`
Datei: `src/Schnittstellenzentrale/OData/ODataEdmModelBuilder.cs`

Statische Hilfsklasse, die das EDM-Modell der eigenen OData-API beschreibt.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `Build()` | `public static` | Erzeugt ein `IEdmModel` mit den Entity-Sets `Applications`, `ApplicationGroups`, `Endpoints`, `EndpointGroups` |

Wird von `ODataImportServiceRealMetadataTests` genutzt, um reale CSDL-XML-Inhalte für Tests zu generieren.

---

## `ODataApplicationsController`
Datei: `src/Schnittstellenzentrale/OData/ODataApplicationsController.cs`

OData-CRUD-Controller für den Entity-Set `Applications` unter `/odatav4`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `Get()` | `public` | Gibt alle Anwendungen zurück (mit `[EnableQuery]`) |
| `Get(int key)` | `public` | Gibt eine Anwendung per ID zurück |
| `Post(Application entity)` | `public` | Legt neue Anwendung an; erkennt `InterfaceType` automatisch |
| `Put(int key, Application entity)` | `public` | Vollständige Aktualisierung; blockiert System-Anwendungen (403) |
| `Patch(int key, JsonElement patch)` | `public` | Partielle Aktualisierung; validiert `IconData` als Base64 |
| `Delete(int key)` | `public` | Löscht Anwendung; blockiert System-Anwendungen (403) |
| `TryApplyPatch(JsonElement patch, Application target, out string? error)` | `private static` | Wendet JSON-Patch auf `Application`-Objekt an |

Erbt von: `ODataControllerBase`

---

## `ODataEndpointsController`
Datei: `src/Schnittstellenzentrale/OData/ODataEndpointsController.cs`

OData-CRUD-Controller für den Entity-Set `Endpoints` unter `/odatav4`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `Get()` | `public` | Gibt alle Endpunkte aller Anwendungen zurück |
| `Get(int key)` | `public` | Gibt einen Endpunkt per ID zurück |
| `Post(Endpoint entity)` | `public` | Legt neuen Endpunkt an; blockiert System-Anwendungen (403) |
| `Put(int key, Endpoint entity)` | `public` | Vollständige Aktualisierung; blockiert System-Anwendungen (403) |
| `Patch(int key, JsonElement patch)` | `public` | Partielle Aktualisierung; blockiert System-Anwendungen (403) |
| `Delete(int key)` | `public` | Löscht Endpunkt; blockiert System-Anwendungen (403) |
| `ApplyPatch(JsonElement patch, Endpoint target)` | `private static` | Wendet JSON-Patch auf `Endpoint`-Objekt an |

Erbt von: `ODataControllerBase`

---

## `ODataApplicationGroupsController`
Datei: `src/Schnittstellenzentrale/OData/ODataApplicationGroupsController.cs`

OData-CRUD-Controller für den Entity-Set `ApplicationGroups` unter `/odatav4`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `Get()` | `public` | Gibt alle Anwendungsgruppen zurück |
| `Get(int key)` | `public` | Gibt eine Gruppe per ID zurück |
| `Post(ApplicationGroup entity)` | `public` | Legt neue Gruppe an |
| `Put(int key, ApplicationGroup entity)` | `public` | Vollständige Aktualisierung; blockiert System-Gruppen (403) |
| `Patch(int key, JsonElement patch)` | `public` | Partielle Aktualisierung; blockiert System-Gruppen (403) |
| `Delete(int key)` | `public` | Löscht Gruppe; blockiert System-Gruppen (403) |

Erbt von: `ODataControllerBase`

---

## `ODataEndpointGroupsController`
Datei: `src/Schnittstellenzentrale/OData/ODataEndpointGroupsController.cs`

OData-CRUD-Controller für den Entity-Set `EndpointGroups` unter `/odatav4`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `Get()` | `public` | Gibt alle Endpunkt-Gruppen zurück |
| `Get(int key)` | `public` | Gibt eine Gruppe per ID zurück |
| `Post(EndpointGroup entity)` | `public` | Legt neue Gruppe an; blockiert System-Anwendungen (403) |
| `Put(int key, EndpointGroup entity)` | `public` | Vollständige Aktualisierung; blockiert System-Anwendungen (403) |
| `Patch(int key, JsonElement patch)` | `public` | Partielle Aktualisierung; blockiert System-Anwendungen (403) |
| `Delete(int key)` | `public` | Löscht Gruppe; blockiert System-Anwendungen (403) |

Erbt von: `ODataControllerBase`

---

## `ODataControllerBase`
Datei: `src/Schnittstellenzentrale/OData/ODataControllerBase.cs`

Abstrakte Basisklasse für alle OData-Controller; implementiert Bearer-Token-Validierung als `IAsyncActionFilter`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `OnActionExecutionAsync(...)` | `public` | Prüft `Authorization: Bearer`-Header; validiert Token via `ITokenStore.ValidateAndRotateAsync`; schreibt neuen Token in Response-Header `X-New-Token` |

---

## `ODataAuthController`
Datei: `src/Schnittstellenzentrale/OData/ODataAuthController.cs`

Authentifizierungsendpunkt unter `/odatav4/authenticate`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `AuthenticateAsync()` | `public` | Nimmt Windows-Identität des Benutzers entgegen und gibt einen Bearer-Token via `ITokenStore.CreateTokenAsync` zurück |

---

## DI-Registrierung
Datei: `src/Schnittstellenzentrale/Program.cs`, Zeile 126

```
builder.Services.AddScoped<IODataImportService, ODataImportService>();
```

`ODataImportService` ist als Scoped-Service registriert — identisch zum `ISwaggerImportService`-Pattern.
