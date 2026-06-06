# Interfaces

## `IODataImportService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IODataImportService.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `ImportAsync` | `Application application` | `Task<ImportDiff>` | Ruft OData-CSDL-Metadaten von `application.InterfaceUrl` ab, parst das Dokument und berechnet den Diff gegenüber den vorhandenen Endpunkten |
| `ApplyDiffAsync` | `ImportDiff diff` | `Task` | Schreibt neue, geänderte und gelöschte Endpunkte aus dem Diff in das Repository |

Implementiert von: `ODataImportService` (`src/Schnittstellenzentrale.Infrastructure/Services/ODataImportService.cs`)

Wird in `ApplicationCard.razor` per `@inject IODataImportService ODataImportService` genutzt. In `ApplicationContentView.razor` fehlt dieser Inject noch.
