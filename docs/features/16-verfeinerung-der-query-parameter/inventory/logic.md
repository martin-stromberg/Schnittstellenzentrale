# Logikklassen

## `EndpointExecutionService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionService.cs`

Implementiert `IEndpointExecutionService`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `ExecuteAsync(Endpoint)` | `public` | Führt den Endpunkt aus; wählt je nach `AuthenticationType` den Ausführungspfad. Einstiegspunkt für `IEndpointExecutionService`. |
| `ExecuteWithAuthAsync(Endpoint)` | `private` | Führt die HTTP-Anfrage mit Standard- oder Negotiate-Auth aus. Ruft intern `BuildRequest` auf. |
| `ExecuteImpersonatedAsync(Endpoint)` | `private` | Führt die Anfrage unter Windows-Impersonation aus. Ruft intern `BuildRequest` auf. |
| `SendAndBuildResultAsync(HttpClient, Endpoint, HttpRequestMessage)` | `private static` | Schickt die Anfrage, misst die Laufzeit und delegiert an `BuildResult`. |
| `BuildResult(Endpoint, HttpResponseMessage, long)` | `private static` | Baut das `EndpointExecutionResult` aus Response, Statuscode, Headern, Body und Laufzeit zusammen. |
| `BuildRequest(Endpoint)` | `private` | Baut die `HttpRequestMessage`. Hängt `QueryParameters` als Query-String an. **Pfad-Platzhalter werden aktuell nicht aufgelöst** — `RelativePath` wird unverändert übernommen. |
| `ApplyAuthentication(HttpRequestMessage, Endpoint)` | `private` | Setzt den `Authorization`-Header für `Basic` und `BearerToken`. |

### Relevante Implementierungsdetails in `BuildRequest`

```csharp
var url = endpoint.Application.BaseUrl.TrimEnd('/') + "/" + endpoint.RelativePath.TrimStart('/');

if (endpoint.QueryParameters.Any())
{
    var query = string.Join("&", endpoint.QueryParameters.Select(p =>
        $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    url += "?" + query;
}
```

- Alle `QueryParameters` werden bedingungslos als Query-String angehängt.
- Es findet keine Unterscheidung zwischen Pfad-Platzhalter-Werten und regulären Query-Parametern statt.
- `Uri.EscapeDataString` wird für Key und Value verwendet.
- `RelativePath` wird nicht auf `{name}`-Platzhalter geprüft oder aufgelöst.

---

## `EndpointPage` (Razor-Komponente, Code-Behind)
Datei: `src/Schnittstellenzentrale/Components/Shared/EndpointPage.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `OnParametersSetAsync()` | `protected override` | Lädt Modell neu, wenn sich die Endpunkt-ID ändert. Ruft `LoadModelFromParameter()` auf. |
| `LoadModelFromParameter()` | `private` | Spiegelt `Endpoint`-Felder in das lokale `_model`. Befüllt `_queryParameters` aus `Endpoint.QueryParameters` (nur `Key`/`Value`). Ruft `SyncAutoContentType()` auf. Kein Aufruf von `SyncPathParameters()` oder `ExtractAndStripQueryString()`. |
| `OnAfterRenderAsync(bool)` | `protected override` | Lädt JS-Modul und registriert Shortcut beim ersten Render. |
| `OnSaveShortcut()` | `public` (JSInvokable) | Wird per JS-Shortcut aufgerufen; delegiert an `SaveAsync()`. |
| `OnNameChanged(ChangeEventArgs)` | `private` | Aktualisiert `_model.Name`, ruft `MarkDirty()` auf. |
| `OnPathChanged(ChangeEventArgs)` | `private` | Aktualisiert `_model.RelativePath`, ruft `MarkDirty()` auf. Kein Blur-Handler vorhanden. |
| `OnMethodChanged(ChangeEventArgs)` | `private` | Aktualisiert `_model.Method`, ruft `MarkDirty()` auf. |
| `OnAuthTypeChanged(AuthenticationType)` | `private` | Aktualisiert `_model.AuthenticationType`, ruft `MarkDirty()` auf. |
| `OnBodyChanged(string?)` | `private` | Aktualisiert `_model.Body`, ruft `MarkDirty()` auf. |
| `OnBodyModeChanged(BodyMode)` | `private` | Aktualisiert `_model.BodyMode`, ruft `SyncAutoContentType()` und `MarkDirty()` auf. |
| `SyncAutoContentType()` | `private` | Verwaltet den automatischen `Content-Type`-Header in `_headers`. |
| `MarkDirty()` | `private` | Setzt `_isDirty = true` und aktiviert Navigations-Guards. |
| `EnableNavigationGuardsAsync()` | `private` | Registriert Location-Changing-Handler und aktiviert `beforeunload`-Guard. |
| `DisableBeforeUnloadGuardAsync()` | `private` | Deaktiviert `beforeunload`-Guard. |
| `UnregisterLocationChanging()` | `private` | Gibt den Location-Changing-Handler frei. |
| `HandleLocationChanging(LocationChangingContext)` | `private` | Zeigt Bestätigungsdialog, wenn ungespeicherte Änderungen vorhanden. |
| `SaveAsync()` | `private` | Baut `_model.Headers` und `_model.QueryParameters` aus den lokalen Listen auf und ruft `PersistAsync()` auf. |
| `ForceSaveAsync()` | `private` | Lädt aktuellen `RowVersion`-Wert und erzwingt Speicherung nach Concurrency-Warnung. |
| `CancelConcurrencyWarning()` | `private` | Schließt den Concurrency-Warndialog. |
| `PersistAsync(Endpoint)` | `private` | Delegiert an `EndpointRepository.AddEndpointAsync` oder `UpdateEndpointAsync`. |
| `OnSaveSuccessAsync()` | `private` | Setzt `_isDirty = false`, deaktiviert Guards, sendet SignalR-Notification, ruft `OnEndpointSaved` auf. |
| `SendRequestAsync()` | `private` | Speichert bei Bedarf, lädt Endpunkt neu und ruft `ExecutionService.ExecuteAsync()` auf. |
| `DisposeAsync()` | `public` | Gibt JS-Modul und DotNet-Referenz frei. |

#### Fehlende Methoden (laut Anforderung neu zu erstellen)

- `SyncPathParameters()` — nicht vorhanden
- `ExtractAndStripQueryString()` — nicht vorhanden
- `OnPathBlur()` — nicht vorhanden
- `ResolveDisplayUrl()` — nicht vorhanden

#### Template-Rendering des Pfad-Felds

```razor
<input class="form-control flex-grow-1" placeholder="Relativer Pfad"
       value="@_model.RelativePath" @oninput="OnPathChanged" />
```

- Zeigt aktuell `_model.RelativePath` direkt (kein `onblur`, keine aufgelöste URL).
- Kein `@onblur`-Handler registriert.

---

## `RequestQueryParamsPanel` (Razor-Komponente, Code-Behind)
Datei: `src/Schnittstellenzentrale/Components/Shared/RequestQueryParamsPanel.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `OnKeyChanged(QueryParamEntry, ChangeEventArgs)` | `private` | Aktualisiert `entry.Key`, ruft `OnChanged` auf. Ausgelöst per `@oninput`. |
| `OnValueChanged(QueryParamEntry, ChangeEventArgs)` | `private` | Aktualisiert `entry.Value`, ruft `OnChanged` auf. Ausgelöst per `@oninput`. |
| `AddParam()` | `private` | Fügt neuen leeren `QueryParamEntry` hinzu, ruft `OnChanged` auf. |
| `RemoveParam(QueryParamEntry)` | `private` | Entfernt Eintrag aus `QueryParameters`, ruft `OnChanged` auf. |

#### Template-Rendering des Löschen-Buttons

```razor
<button type="button" class="btn btn-outline-danger btn-sm" @onclick="() => RemoveParam(param)">&#x2715;</button>
```

- Löschen-Button wird für **alle** Einträge gerendert — keine Unterscheidung nach `IsPathParameter`.
- Events nur per `@oninput`, kein `@onblur`.
