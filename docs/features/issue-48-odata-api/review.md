# Plan-Review

## Ergebnis

**Status:** Vollständig umgesetzt

## Umgesetzte Planelemente

- [x] `ApplicationCard.razor` (Blazor-Komponente) — vollständig gelöscht; keine Referenzen verbleiben in `src/`
- [x] `ApplicationCard_*`-Schlüssel in `SharedResources.resx` (EN) — alle 7 Schlüssel entfernt; kein `ApplicationCard_`-Eintrag mehr vorhanden
- [x] `ApplicationCard_*`-Schlüssel in `SharedResources.de.resx` (DE) — alle 7 Schlüssel entfernt; kein `ApplicationCard_`-Eintrag mehr vorhanden
- [x] Schlüssel `ApplicationContentView_Button_ODataImport` in `SharedResources.resx` (EN) — Wert `OData Import`, Comment gesetzt
- [x] Schlüssel `ApplicationContentView_Button_ODataImport` in `SharedResources.de.resx` (DE) — Wert `OData-Import`, Comment gesetzt
- [x] Injection `@inject IODataImportService ODataImportService` in `ApplicationContentView.razor` — vorhanden (Zeile 3)
- [x] Feld `_showODataImport` (`bool`) in `ApplicationContentView` — vorhanden
- [x] Feld `_odataDiff` (`ImportDiff?`) in `ApplicationContentView` — vorhanden
- [x] Feld `_errorMessage` (`string?`) in `ApplicationContentView` — vorhanden
- [x] Methode `OpenODataImportAsync` in `ApplicationContentView` — vorhanden; setzt `_errorMessage = null`, awaitet `ODataImportService.ImportAsync`, verzweigt nach `ErrorMessage`, setzt `_odataDiff` und `_showODataImport = true` bei Erfolg
- [x] Methode `CloseODataImport` in `ApplicationContentView` — vorhanden; setzt `_showODataImport = false`
- [x] Inline-Fehleranzeige (`_errorMessage`) im Markup von `ApplicationContentView` — vorhanden (roter Alert im `sz-hero-content`-Bereich)
- [x] Bedingter OData-Import-Button im `sz-hero-right`-Abschnitt (Bedingung: `InterfaceType.OData` und `InterfaceUrl` nicht leer) — vorhanden
- [x] Bedingte Einbindung `ODataImportDialog` (Bedingung: `_showODataImport && _odataDiff != null`) — vorhanden
- [x] Testklasse `ApplicationContentViewTests` (bUnit) — angelegt unter `src/Schnittstellenzentrale.Tests/Components/ApplicationContentViewTests.cs`
- [x] Testmethode `ODataImportButton_VisibleForODataApplication` in `ApplicationContentViewTests` — vorhanden
- [x] Testmethode `ODataImportButton_HiddenForRestApplication` in `ApplicationContentViewTests` — vorhanden
- [x] Testmethode `ODataImportButton_HiddenWhenInterfaceUrlEmpty` in `ApplicationContentViewTests` — vorhanden
- [x] Testmethode `OpenODataImport_OnError_ShowsErrorMessage` in `ApplicationContentViewTests` — vorhanden
- [x] Testmethode `OpenODataImport_OnSuccess_OpensDialog` in `ApplicationContentViewTests` — vorhanden
- [x] E2E-Tests `ODataImportTests.ImportOData_RecognizesODataType_AndImportsEndpoints` und `ImportOData_CrudOperation_PersistsChange` — Testdatei `ODataImportTests.cs` ist vorhanden

## Offene Aufgaben

Keine.

## Hinweise

- Der Inline-Fehleralert (`sz-alert sz-alert-danger`) ist innerhalb von `sz-hero-content`, jedoch außerhalb von `sz-hero-right` platziert. Dies weicht geringfügig von der Planformulierung ab („platziert im `sz-hero-right`-Bereich oder unmittelbar darunter"), ist aber durch das „oder unmittelbar darunter" gedeckt und entspricht der Referenzimplementierung aus `ApplicationCard.razor`.
- Die E2E-Testdatei `ODataImportTests.cs` war laut Plan bereits vorhanden und musste nicht neu angelegt werden — dies entspricht der Planaussage.
