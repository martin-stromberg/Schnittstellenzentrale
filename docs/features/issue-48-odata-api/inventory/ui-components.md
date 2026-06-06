# UI-Komponenten

## `ApplicationContentView.razor`
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationContentView.razor`

Die primär betroffene Detailansichtskomponente. Injiziert aktuell `IApplicationService`, `ISwaggerImportService`, `IHealthCheckService` und `IApplicationApiClient`.

### Vorhandene Felder im `@code`-Block

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Application` | `Application` (Parameter) | Die angezeigte Anwendung |
| `_endpointCount` | `int` | Anzahl der Endpunkte |
| `_showSwaggerImport` | `bool` | Steuert die Sichtbarkeit des Swagger-Import-Dialogs |
| `_showHealthCheck` | `bool` | Steuert die Sichtbarkeit des HealthCheck-Dialogs |
| `_swaggerDiff` | `ImportDiff?` | Ergebnis des Swagger-Imports |
| `_healthCheckResult` | `bool?` | Ergebnis des Health Checks |

### Vorhandene Methoden

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `OnParametersSetAsync` | `protected override` | Lädt Endpunktanzahl via `ApplicationApiClient.GetEndpointsAsync` |
| `OnNameChanged(string)` | `private async` | Ruft `ApplicationService.UpdateNameAsync` auf |
| `OnSubtitleChanged(string?)` | `private async` | Ruft `ApplicationService.UpdateSubtitleAsync` auf |
| `OnIconChanged(byte[])` | `private async` | Ruft `ApplicationService.UpdateIconAsync` auf |
| `OpenSwaggerImportAsync()` | `private async` | Setzt `_swaggerDiff` via `SwaggerImportService.ImportAsync`, setzt `_showSwaggerImport = true` |
| `RunHealthCheckAsync()` | `private async` | Setzt `_healthCheckResult` via `HealthCheckService.CheckAsync`, setzt `_showHealthCheck = true` |
| `CloseSwaggerImport()` | `private` | Setzt `_showSwaggerImport = false` |
| `CloseHealthCheck()` | `private` | Setzt `_showHealthCheck = false` |
| `OnHealthCheckRemove()` | `private async` | Schließt Dialog, ruft `ApplicationApiClient.DeleteApplicationAsync` auf |

### Fehlende Elemente (bezogen auf OData-Anforderung)

- Kein `@inject IODataImportService ODataImportService`
- Kein Feld `_showODataImport` (bool)
- Kein Feld `_odataDiff` (`ImportDiff?`)
- Keine Methode `OpenODataImportAsync()`
- Keine Methode `CloseODataImport()`
- Kein bedingter OData-Import-Button im `sz-hero-right`-Abschnitt
- Keine `ODataImportDialog`-Einbindung

### Vorhandene bedingte Anzeige im `sz-hero-right`

```razor
@if (Application.InterfaceType == Core.Enums.InterfaceType.Rest && !string.IsNullOrEmpty(Application.InterfaceUrl))
{
    <button type="button" class="sz-btn sz-btn-outline sz-btn-sm" @onclick="OpenSwaggerImportAsync">
        @L["ApplicationContentView_Button_SwaggerImport"]
    </button>
}
```
Diese Bedingung ist das direkte Vorbild für den OData-Button.

---

## `ApplicationCard.razor`
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationCard.razor`

Die ältere Kartenkomponente, die den OData-Import-Button bereits vollständig implementiert. Injiziert `IApplicationApiClient`, `ISwaggerImportService`, `IODataImportService` und `IHealthCheckService`.

### Vorhandene Felder im `@code`-Block

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `ApplicationId` | `int` (Parameter) | ID der anzuzeigenden Anwendung |
| `OnApplicationRemoved` | `EventCallback` (Parameter) | Callback wenn Anwendung entfernt wird |
| `_application` | `Application?` | Die geladene Anwendung |
| `_errorMessage` | `string?` | Inline-Fehlermeldung |
| `_showSwaggerImport` | `bool` | Sichtbarkeit des Swagger-Import-Dialogs |
| `_showODataImport` | `bool` | Sichtbarkeit des OData-Import-Dialogs |
| `_showHealthCheck` | `bool` | Sichtbarkeit des HealthCheck-Dialogs |
| `_swaggerDiff` | `ImportDiff?` | Ergebnis des Swagger-Imports |
| `_odataDiff` | `ImportDiff?` | Ergebnis des OData-Imports |
| `_healthCheckResult` | `bool?` | Ergebnis des Health Checks |

### Vorhandene Methoden

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `OnParametersSetAsync` | `protected override` | Lädt Anwendung via `ApplicationApiClient.GetApplicationByIdAsync` |
| `OpenSwaggerImport()` | `private async` | Setzt `_swaggerDiff` via `SwaggerImportService.ImportAsync` |
| `OpenODataImport()` | `private async` | Setzt `_odataDiff` via `ODataImportService.ImportAsync`, prüft `ErrorMessage` |
| `RunHealthCheck()` | `private async` | Setzt `_healthCheckResult` via `HealthCheckService.CheckAsync` |
| `CloseSwaggerImport()` | `private` | Setzt `_showSwaggerImport = false` |
| `CloseODataImport()` | `private` | Setzt `_showODataImport = false` |
| `CloseHealthCheck()` | `private` | Setzt `_showHealthCheck = false` |
| `RemoveApplication()` | `private async` | Löscht Anwendung, feuert `OnApplicationRemoved` |

### Besonderheit: Fehlerbehandlung in `OpenODataImport`

`ApplicationCard` prüft `_odataDiff.ErrorMessage` nach dem Import und zeigt bei Fehler eine inline `alert`-Meldung an, ohne den Dialog zu öffnen. Dieses Muster fehlt in `ApplicationContentView`.

---

## `ODataImportDialog.razor`
Datei: `src/Schnittstellenzentrale/Components/Shared/ODataImportDialog.razor`

Existiert vollständig. Delegiert an die generische `ImportDialog`-Komponente und ruft `ODataImportService.ApplyDiffAsync` auf.

### Parameter

| Parameter | Typ | Beschreibung |
|-----------|-----|--------------|
| `Diff` | `ImportDiff` | Das Import-Diff-Ergebnis |
| `Application` | `Application` | Die Zielanwendung |
| `OnClose` | `EventCallback` | Callback beim Schließen |

---

## `SwaggerImportDialog.razor`
Datei: `src/Schnittstellenzentrale/Components/Shared/SwaggerImportDialog.razor`

Strukturell identisch mit `ODataImportDialog`. Bereits korrekt in `ApplicationContentView` eingebunden — dient als direktes Vorbild für die OData-Dialog-Einbindung.
