# UI-Komponenten

## `ApplicationCard.razor`
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationCard.razor`

Zeigt die Detailansicht einer Anwendung und enthält Buttons für Swagger-Import, OData-Import und Health-Check.

**Parameter:**
| Parameter | Typ | Zweck |
|-----------|-----|-------|
| `ApplicationId` | `int` | ID der anzuzeigenden Anwendung |
| `OnApplicationRemoved` | `EventCallback` | Wird ausgelöst, wenn die Anwendung gelöscht wird |

**State-Felder:**
| Feld | Typ | Zweck |
|------|-----|-------|
| `_application` | `Application?` | Geladene Anwendungsdaten |
| `_errorMessage` | `string?` | Fehlermeldung bei Ladefehler |
| `_showSwaggerImport` | `bool` | Steuert Sichtbarkeit des Swagger-Import-Dialogs |
| `_showODataImport` | `bool` | Steuert Sichtbarkeit des OData-Import-Dialogs |
| `_showHealthCheck` | `bool` | Steuert Sichtbarkeit des Health-Check-Dialogs |
| `_swaggerDiff` | `ImportDiff?` | Ergebnis des letzten Swagger-Imports |
| `_odataDiff` | `ImportDiff?` | Ergebnis des letzten OData-Imports |
| `_healthCheckResult` | `bool?` | Ergebnis des letzten Health-Checks |

**Methoden:**
| Methode | Zweck |
|---------|-------|
| `OnParametersSetAsync` | Lädt `_application` via `IApplicationApiClient.GetApplicationByIdAsync` |
| `OpenSwaggerImport` | Ruft `ISwaggerImportService.ImportAsync` auf, setzt `_swaggerDiff` und `_showSwaggerImport = true` |
| `OpenODataImport` | Ruft `IODataImportService.ImportAsync` auf, setzt `_odataDiff` und `_showODataImport = true` |
| `RunHealthCheck` | Ruft `IHealthCheckService.CheckAsync` auf |
| `CloseSwaggerImport` | Setzt `_showSwaggerImport = false` |
| `CloseODataImport` | Setzt `_showODataImport = false` |
| `CloseHealthCheck` | Setzt `_showHealthCheck = false` |
| `RemoveApplication` | Löscht Anwendung via `IApplicationApiClient.DeleteApplicationAsync` |

**Inject-Abhängigkeiten:** `IApplicationApiClient`, `ISwaggerImportService`, `IODataImportService`, `IHealthCheckService`, `IStringLocalizer<SharedResources>`

**Button-Konditionalität:**
- Swagger-Import-Button nur sichtbar bei `InterfaceType.Rest`
- OData-Import-Button nur sichtbar bei `InterfaceType.OData`

---

## `ODataImportDialog.razor`
Datei: `src/Schnittstellenzentrale/Components/Shared/ODataImportDialog.razor`

Schlanke Wrapper-Komponente; delegiert vollständig an `ImportDialog` und ruft `IODataImportService.ApplyDiffAsync` auf.

**Parameter:**
| Parameter | Typ | Zweck |
|-----------|-----|-------|
| `Diff` | `ImportDiff` | Diff-Ergebnis aus `ODataImportService.ImportAsync` |
| `Application` | `Application` | Zugehörige Anwendung |
| `OnClose` | `EventCallback` | Wird beim Schließen des Dialogs ausgelöst |

**Methoden:**
| Methode | Zweck |
|---------|-------|
| `ApplyAsync(ImportDiff diff)` | Ruft `IODataImportService.ApplyDiffAsync(diff)` auf |

**Inject-Abhängigkeiten:** `IODataImportService`, `IStringLocalizer<SharedResources>`

Analoges Pattern zu `SwaggerImportDialog.razor`.

---

## `ImportDialog.razor`
Datei: `src/Schnittstellenzentrale/Components/Shared/ImportDialog.razor`

Wiederverwendbarer Dialog für Import-Vorschau und selektive Übernahme von Endpunkten. Wird sowohl von `SwaggerImportDialog` als auch von `ODataImportDialog` genutzt.

**Parameter:**
| Parameter | Typ | Zweck |
|-----------|-----|-------|
| `Title` | `string` | Dialog-Titel (z. B. „OData-Import-Vorschau") |
| `Diff` | `ImportDiff` | Darzustellender Diff |
| `Application` | `Application` | Zugehörige Anwendung |
| `OnClose` | `EventCallback` | Schließt den Dialog |
| `OnApply` | `EventCallback<ImportDiff>` | Übergibt den selektierten Diff zur Übernahme |

**State-Felder:**
| Feld | Typ | Zweck |
|------|-----|-------|
| `_selectedNew` | `HashSet<Endpoint>` | Ausgewählte neue Endpunkte |
| `_selectedChanged` | `HashSet<Endpoint>` | Ausgewählte geänderte Endpunkte |
| `_selectedRemoved` | `HashSet<Endpoint>` | Ausgewählte zu entfernende Endpunkte |
| `_errorMessage` | `string?` | Fehlermeldung bei Apply-Fehler |

**Funktionalität:**
- Zeigt drei Abschnitte (Neu / Geändert / Entfernt) mit Checkboxen zur selektiven Auswahl
- Erstellt bei „Übernehmen" einen gefilterten `ImportDiff` aus den Selektionen und ruft `OnApply` auf
- Behandelt `DbUpdateConcurrencyException` und allgemeine Exceptions mit lokalisierten Fehlermeldungen

---

## Lokalisierungsschlüssel (SharedResources)

Folgende Schlüssel für das OData-Import-Feature sind in beiden resx-Dateien vorhanden:

| Schlüssel | DE-Wert | EN-Wert |
|-----------|---------|---------|
| `ODataImportDialog_Title` | `OData-Import-Vorschau` | `OData Import Preview` |
| `ApplicationCard_Button_ODataImport` | `OData-Import` | `OData Import` |
| `ApplicationCard_Label_MetadataUrl` | _(vorhanden)_ | _(vorhanden)_ |
| `ApplicationEditor_Hint_OData` | `OData` | `OData` |
