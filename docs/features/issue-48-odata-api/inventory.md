# Bestandsaufnahme: OData-Import-Button in der Detailansicht

Analysiert wurde der Bereich rund um den OData-Import in der Detailansichtskomponente `ApplicationContentView`, die ältere `ApplicationCard`, den zugehörigen `ODataImportDialog`, den `IODataImportService` sowie die Lokalisierungsdateien und Testabdeckung — bezogen auf die Anforderung, den fehlenden OData-Import-Button in `ApplicationContentView` zu ergänzen.

## Zusammenfassung

- `ApplicationContentView.razor` enthält bereits den vollständigen Swagger-Import-Button (bedingt für `InterfaceType.Rest`) sowie den HealthCheck-Button, aber **keinen OData-Import-Button**.
- `ApplicationContentView.razor` injiziert `ISwaggerImportService`, aber **nicht** `IODataImportService`.
- Die Felder `_showODataImport` und `_odataDiff` sowie die Methoden `OpenODataImportAsync` und `CloseODataImport` fehlen in `ApplicationContentView` vollständig.
- `ODataImportDialog.razor` ist vollständig implementiert und wird nur in `ApplicationCard` genutzt — nicht in `ApplicationContentView`.
- `ApplicationCard.razor` enthält die vollständige Referenzimplementierung des OData-Imports: Button, Dialog, Felder, Methoden und Fehlerbehandlung via inline-`_errorMessage`. Diese Komponente dient als direktes Implementierungsvorbild.
- `IODataImportService` ist vollständig definiert (`ImportAsync` / `ApplyDiffAsync`) und korrekt implementiert in `ODataImportService`.
- Der Lokalisierungsschlüssel `ApplicationContentView_Button_ODataImport` fehlt in beiden resx-Dateien. Als Vorlage dient `ApplicationCard_Button_ODataImport` (`"OData Import"` / `"OData-Import"`).
- Die Playwright-Tests (`ODataImportTests`) suchen den Button per Rollenname `"OData-Import"` in der Detailansicht — sie sind also bereits auf `ApplicationContentView` ausgerichtet, scheitern jedoch mangels Button.
- Es gibt keine Unit- oder Bunit-Tests für `ApplicationContentView`.

## Details

- [UI-Komponenten](inventory/ui-components.md) — `ApplicationContentView`, `ApplicationCard`, `ODataImportDialog`, `SwaggerImportDialog`
- [Interfaces](inventory/interfaces.md) — `IODataImportService`
- [Logik](inventory/logic.md) — `ODataImportService` (Implementierung, Fehlerbehandlung)
- [Lokalisierung](inventory/localization.md) — Vorhandene und fehlende resx-Schlüssel
- [Tests](inventory/tests.md) — Playwright-Tests, Unit-Tests, Integrationstests
