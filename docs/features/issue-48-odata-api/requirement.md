# Anforderung – OData-Import-Button in der Detailansicht

## Fachliche Zusammenfassung

In der Detailansicht einer Anwendung (`ApplicationContentView`) fehlt der Button zum Auslösen des OData-Imports. Der Button ist zwar in der älteren `ApplicationCard`-Komponente implementiert und an `IODataImportService.ImportAsync` / `ApplyDiffAsync` gebunden, wurde jedoch beim Umbau auf die neue Detailansicht nicht übernommen. Die `ApplicationContentView` soll um einen OData-Import-Button ergänzt werden, der analog zum vorhandenen Swagger-Import-Button bedingt angezeigt wird – nämlich genau dann, wenn `Application.InterfaceType == InterfaceType.OData` und `Application.InterfaceUrl` nicht leer ist. Nach dem Klick öffnet sich der bereits vorhandene `ODataImportDialog`.

## Betroffene Klassen und Komponenten

### UI-Komponenten
- **`ApplicationContentView.razor`** – Primär betroffene Komponente: Hier fehlt der OData-Import-Button im `sz-hero-right`-Bereich sowie die zugehörige Dialog-Einbindung (`ODataImportDialog`), die Felder `_showODataImport` / `_odataDiff` und die Methoden `OpenODataImportAsync` / `CloseODataImport`.
- **`ODataImportDialog.razor`** – Existiert bereits; muss nur in `ApplicationContentView` eingebunden werden.

### Services / Interfaces
- **`IODataImportService`** – Existiert bereits (`ImportAsync`, `ApplyDiffAsync`); wird in `ApplicationContentView` per `@inject` hinzugefügt.

### Lokalisierung
- **`SharedResources.resx` / `SharedResources.de.resx`** – Neuer Schlüssel `ApplicationContentView_Button_ODataImport` (analog zu `ApplicationContentView_Button_SwaggerImport`) wird benötigt. Der Schlüssel `ApplicationCard_Button_ODataImport` aus der alten Karte kann als Vorlage dienen.

### Tests
- **Playwright-Tests** (`ODataImportTests`) – Bestehende Tests prüfen ggf. die alte `ApplicationCard`; ggf. müssen Selektoren auf die neue Detailansicht angepasst werden.
- **Unit-Tests / Integrationstests** für `ApplicationContentView` – Neue Testfälle, die sicherstellen, dass der Button für `InterfaceType.OData` sichtbar und für andere Typen ausgeblendet ist.

## Implementierungsansatz

Die Änderung ist eng an das bestehende Muster des Swagger-Import-Buttons in `ApplicationContentView` angelehnt:

1. **`@inject IODataImportService ODataImportService`** am Kopf der Komponente ergänzen.
2. Im `sz-hero-right`-Abschnitt einen bedingten Button einfügen:
   ```razor
   @if (Application.InterfaceType == Core.Enums.InterfaceType.OData && !string.IsNullOrEmpty(Application.InterfaceUrl))
   {
       <button type="button" class="sz-btn sz-btn-outline sz-btn-sm" @onclick="OpenODataImportAsync">@L["ApplicationContentView_Button_ODataImport"]</button>
   }
   ```
3. Den `ODataImportDialog` analog zum `SwaggerImportDialog` einbinden (bedingt über `_showODataImport && _odataDiff != null`).
4. Felder `_showODataImport` (bool) und `_odataDiff` (`ImportDiff?`) im `@code`-Block ergänzen.
5. Methoden `OpenODataImportAsync` und `CloseODataImport` implementieren – identisch zur Logik in `ApplicationCard.razor`.
6. Lokalisierungsschlüssel `ApplicationContentView_Button_ODataImport` in beide resx-Dateien eintragen.

Die Klasse `Application.DetectInterfaceType` (in `Application.cs`) erkennt OData-URLs bereits korrekt über den Substring `$metadata`; der `InterfaceType` wird beim Anlegen und Bearbeiten einer Anwendung serverseitig gesetzt und über `IApplicationApiClient` bereitgestellt. Es besteht kein Änderungsbedarf am Datenmodell oder an den Services.

## Konfiguration

Keine zusätzliche Konfiguration erforderlich. Die Sichtbarkeit des Buttons ergibt sich vollständig aus dem gespeicherten `InterfaceType` der Anwendung.

## Offene Fragen

- **Fehlerfeedback:** Soll ein Fehler beim OData-Import (z. B. nicht erreichbare `$metadata`-URL) in `ApplicationContentView` genauso wie in `ApplicationCard` über ein inline `alert`-Element angezeigt werden, oder ist ein anderes Feedback-Muster (z. B. Toast-Notification) gewünscht?
- **Veraltete `ApplicationCard`-Komponente:** Nachdem `ApplicationContentView` die Funktionalität vollständig übernimmt, ist zu klären, ob `ApplicationCard.razor` noch aktiv genutzt wird oder entfernt werden kann, um doppelte Pflege zu vermeiden.
- **Playwright-Testabdeckung:** Müssen bestehende `ODataImportTests` angepasst werden, weil sie bisher gegen `ApplicationCard` geschrieben wurden und jetzt gegen `ApplicationContentView` testen sollen?
