# UI-Komponenten

## `EndpointPage`
Datei: `src/Schnittstellenzentrale/Components/Shared/EndpointPage.razor`

Die Hauptkomponente zur Anzeige und Bearbeitung eines Endpunkts.

### Lokales Modell `_model`

Das lokale `_model`-Objekt ist eine Kopie des `Endpoint`-Parameters. `LoadModelFromParameter` kopiert folgende Felder:

| Feld | Typ | Bemerkung |
|---|---|---|
| `Id` | `int` | |
| `Name` | `string` | |
| `Method` | `HttpMethod` | |
| `RelativePath` | `string` | |
| `Body` | `string?` | |
| `BodyMode` | `BodyMode` | |
| `AuthenticationType` | `AuthenticationType` | |
| `ApplicationId` | `int` | |
| `Application` | `Application` | |
| `EndpointGroupId` | `int?` | |
| `EndpointGroup` | `EndpointGroup?` | |
| `RowVersion` | `byte[]` | |

Die Felder `PreRequestScript` und `PostRequestScript` sind **noch nicht** im `_model` vorhanden.

### Bestehende Registerkarten (Request-Panel)

Die `<ul class="nav nav-tabs">`-Liste enthält aktuell vier Registerkarten:

| Tab-Key | Bezeichnung | Panel-Komponente |
|---|---|---|
| `"auth"` | Autorisierung | `RequestAuthPanel` |
| `"headers"` | Headers | `RequestHeadersPanel` |
| `"query"` | Query-Parameter | `RequestQueryParamsPanel` |
| `"body"` | Body | `RequestBodyPanel` |

Die zwei neuen Registerkarten „Pre-Request-Skript" und „Post-Request-Skript" fehlen noch vollständig.

### Response-Bereich

`_result.ErrorMessage` wird bereits als `<div class="alert alert-danger">` angezeigt — das bestehende Markup kann für Skript-Fehlermeldungen genutzt werden.

### Relevante Methoden

| Methode | Zweck |
|---|---|
| `LoadModelFromParameter()` | Initialisiert `_model` aus dem `Endpoint`-Parameter; **muss um `PreRequestScript`/`PostRequestScript` erweitert werden** |
| `SaveAsync()` | Speichert; baut Headers und QueryParameters aus den lokalen Listen; **muss Skriptfelder übernehmen** |
| `MarkDirty()` | Markiert Änderungen und aktiviert Navigations-Guards |
| `SendRequestAsync()` | Führt Speichern und anschließend `ExecutionService.ExecuteAsync` aus |
