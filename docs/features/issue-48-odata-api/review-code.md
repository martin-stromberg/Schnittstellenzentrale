# Code-Review: issue-48-odata-api (Iteration 3)

Diff-Basis: `git diff main...HEAD`
Branch: `issue-48-odata-api` vs `main`
Datum: 2026-06-07
Effort: high

## Überblick

Iteration 3 behob die Iteration-2-Befunde: IsSystem-Prüfung für `targetGroup` in `PATCH ODataEndpointsController` ergänzt, CLAUDE.md-Ausnahme für Import-Services entfernt, API-First-Refactoring des Import-Flows abgeschlossen (`ApplicationImportController` + neue `IApplicationApiClient`-Methoden). Die `RowVersion`-Logik wurde bereits in Iteration 2 in allen vier Controllern vereinheitlicht. Die `authenticate`-Endpunkt-Duplizierungs-Logik wurde durch den generischen Operationen-Loop ersetzt.

## Akzeptierte Befunde (bekannte Designentscheidungen)

- **Concurrency opt-in**: `entity.RowVersion.Length > 0 ? entity.RowVersion : existing.RowVersion` — bewusstes Fallback-Verhalten, dokumentiert in allen vier Controllern. Clients ohne RowVersion-Unterstützung können schreiben; Clients mit RowVersion erhalten echten Concurrency-Schutz.

## Neue Befunde

```json
[
  {
    "file": "src/Schnittstellenzentrale/Components/Shared/ApplicationContentView.razor",
    "line": 136,
    "summary": "OpenSwaggerImportAsync prüft ErrorMessage nicht — Swagger-Dialog öffnet sich auch bei Import-Fehler.",
    "failure_scenario": "SwaggerImportService.ImportAsync gibt ImportDiff { ErrorMessage = 'Connection refused' } zurück. Der Controller gibt 200 OK mit diesem Diff zurück. ApplicationContentView.OpenSwaggerImportAsync setzt _swaggerDiff = diff und _showSwaggerImport = true, ohne ErrorMessage zu prüfen. Der SwaggerImportDialog öffnet sich mit einem fehlerhaften Diff statt die Fehlermeldung im Hero-Bereich anzuzeigen. Gegensatz: OpenODataImportAsync (Zeile 142–152) prüft result.ErrorMessage korrekt und zeigt _errorMessage an."
  },
  {
    "file": "src/Schnittstellenzentrale/Controllers/ApplicationImportController.cs",
    "line": 47,
    "summary": "ImportSwaggerAsync gibt 200 OK zurück auch wenn diff.ErrorMessage != null — HTTP-Semantik spiegelt Fehlerzustand nicht wider.",
    "failure_scenario": "SwaggerImportService.ImportAsync scheitert (z. B. Timeout, ungültiges JSON). Der Service gibt ImportDiff { ErrorMessage = '...' } zurück. Der Controller gibt 200 OK mit diesem Diff zurück. Der API-Consumer (ApplicationApiClient) erhält eine erfolgreiche HTTP-Antwort und muss intern ErrorMessage prüfen, um den Fehler zu erkennen — das tut der OData-Pfad in ApplicationContentView, aber nicht der Swagger-Pfad (siehe vorherigen Befund). ImportODataAsync (Zeile 70) hat dieselbe Charakteristik; die fehlende Fehlerprüfung in OpenSwaggerImportAsync macht diesen Befund in der Praxis wirksam."
  },
  {
    "file": "src/Schnittstellenzentrale/Components/Shared/ApplicationContentView.razor",
    "line": 138,
    "summary": "OpenSwaggerImportAsync löscht _errorMessage nicht vor dem Aufruf — eine vorherige OData-Fehlermeldung bleibt nach erfolgreichem Swagger-Import sichtbar.",
    "failure_scenario": "Nutzer klickt 'OData-Import', erhält Fehler (_errorMessage = 'Connection refused'). Nutzer klickt danach 'Swagger-Import'. OpenSwaggerImportAsync setzt _errorMessage nicht zurück (im Gegensatz zu OpenODataImportAsync, die _errorMessage = null setzt, Zeile 144). Die OData-Fehlermeldung bleibt im Hero sichtbar, während der Swagger-Import-Dialog geöffnet ist."
  }
]
```

## Zusammenfassung

Die drei verbleibenden Befunde hängen zusammen: Die API-First-Refactoring hat den Swagger-Import-Pfad in `ApplicationContentView` nicht vollständig an das neue Muster angepasst. Der OData-Pfad wurde korrekt implementiert (ErrorMessage-Prüfung, _errorMessage-Reset), aber der Swagger-Pfad wurde nicht entsprechend aktualisiert.

**Empfohlene Korrekturen in `ApplicationContentView.razor` (`OpenSwaggerImportAsync`):**
```csharp
private async Task OpenSwaggerImportAsync()
{
    _errorMessage = null;                           // Befund 3: Reset wie im OData-Pfad
    var result = await ApplicationApiClient.ImportSwaggerMetadataAsync(Application.Id);
    if (result.ErrorMessage != null)                // Befund 1: ErrorMessage prüfen
    {
        _errorMessage = result.ErrorMessage;
        return;
    }
    _swaggerDiff = result;
    _showSwaggerImport = true;
}
```

Befund 2 (HTTP 200 bei Fehler) ist eine API-Design-Entscheidung: Entweder gibt der Controller bei gesetztem ErrorMessage einen 422/503 zurück, oder der Client prüft konsequent ErrorMessage. Die aktuelle Mischung (OData-Pfad prüft, Swagger-Pfad nicht) ist inkonsistent.
