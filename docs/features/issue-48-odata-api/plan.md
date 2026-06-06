# Umsetzungsplan: OData-Import-Button in der Detailansicht

## Übersicht

`ApplicationContentView.razor` wird um einen OData-Import-Button ergänzt, der analog zum bestehenden Swagger-Import-Button bedingt angezeigt wird — genau dann, wenn `Application.InterfaceType == InterfaceType.OData` und `Application.InterfaceUrl` nicht leer ist. Nach dem Klick öffnet sich der bereits vorhandene `ODataImportDialog`. Im gleichen Zug wird die veraltete `ApplicationCard.razor` vollständig entfernt, da sie in keiner anderen Razor-Komponente oder C#-Datei als `<ApplicationCard` eingebunden ist. Die Änderung berührt weder das Datenmodell noch Services oder das Datenbankschema.

## Designentscheidungen

Keine — folgt bestehenden Mustern.

Die Fehlerbehandlung (`_errorMessage`) wird analog zu `ApplicationCard.razor` übernommen: Bei gesetztem `ImportDiff.ErrorMessage` wird kein Dialog geöffnet, sondern eine inline-Fehlermeldung im `sz-hero-right`-Bereich angezeigt (roter Alert in der Komponente, keine Toast-Notification).

## Programmabläufe

### OData-Import öffnen

1. Nutzer klickt den OData-Import-Button im `sz-hero-right`-Abschnitt von `ApplicationContentView`.
2. `OpenODataImportAsync` wird aufgerufen; `_errorMessage` wird auf `null` zurückgesetzt.
3. `ODataImportService.ImportAsync(Application)` wird awaited.
4. Wenn `ImportDiff.ErrorMessage` nicht null ist: `_errorMessage` wird gesetzt, kein Dialog wird geöffnet, Methode endet.
5. Wenn kein Fehler: `_odataDiff` wird mit dem Ergebnis belegt, `_showODataImport` wird auf `true` gesetzt.
6. Blazor rendert den `ODataImportDialog`, der `_odataDiff` als `Diff` und `Application` als `Application` erhält.

Beteiligte Klassen/Komponenten: `ApplicationContentView`, `IODataImportService`, `ODataImportDialog`

### OData-Import schließen

1. `ODataImportDialog` feuert den `OnClose`-Callback.
2. `CloseODataImport` wird aufgerufen.
3. `_showODataImport` wird auf `false` gesetzt.
4. Blazor blendet den `ODataImportDialog` aus.

Beteiligte Klassen/Komponenten: `ApplicationContentView`, `ODataImportDialog`

## Neue Klassen

Keine.

## Änderungen an bestehenden Klassen

### `ApplicationContentView.razor` (Blazor-Komponente)

- **Neue Injection:** `IODataImportService ODataImportService` — Zugriff auf den OData-Import-Service
- **Neue Eigenschaften:**
  - `_showODataImport` (`bool`) — steuert die Sichtbarkeit des `ODataImportDialog`
  - `_odataDiff` (`ImportDiff?`) — hält das Ergebnis des letzten OData-Imports
  - `_errorMessage` (`string?`) — hält eine inline-Fehlermeldung bei fehlgeschlagenem Import
- **Neue Methoden:**
  - `OpenODataImportAsync` — setzt `_errorMessage = null`, awaitet `ODataImportService.ImportAsync`, setzt `_odataDiff` und `_showODataImport = true` bei Erfolg; setzt `_errorMessage` bei Fehler
  - `CloseODataImport` — setzt `_showODataImport = false`
- **Markup-Ergänzungen:**
  - Inline-Fehleranzeige für `_errorMessage` als roter Alert (analog zu `ApplicationCard.razor`) — platziert im `sz-hero-right`-Bereich oder unmittelbar darunter
  - Bedingter OData-Import-Button im `sz-hero-right`-Abschnitt (Bedingung: `InterfaceType.OData` und `InterfaceUrl` nicht leer), analog zum Swagger-Import-Button
  - Bedingte Einbindung von `ODataImportDialog` (Bedingung: `_showODataImport && _odataDiff != null`), analog zur `SwaggerImportDialog`-Einbindung

### `SharedResources.resx` (EN Fallback)

- **Neuer Schlüssel:** `ApplicationContentView_Button_ODataImport` — Wert: `OData Import`; Comment: `Button label for triggering the OData import in the application detail view (ApplicationContentView)`
- **Entfernte Schlüssel:** Alle `ApplicationCard_*`-Schlüssel (`ApplicationCard_Error_LoadApplication`, `ApplicationCard_Button_SwaggerImport`, `ApplicationCard_Button_ODataImport`, `ApplicationCard_Button_HealthCheck`, `ApplicationCard_Label_BaseUrl`, `ApplicationCard_Label_MetadataUrl`, `ApplicationCard_Label_SwaggerUrl`) — werden mit Löschen von `ApplicationCard.razor` obsolet

### `SharedResources.de.resx` (DE)

- **Neuer Schlüssel:** `ApplicationContentView_Button_ODataImport` — Wert: `OData-Import`; Comment: `Beschriftung des Buttons zum Auslösen des OData-Imports in der Anwendungsdetailansicht (ApplicationContentView)`
- **Entfernte Schlüssel:** Alle `ApplicationCard_*`-Schlüssel (analog zur EN-Datei)

### `ApplicationCard.razor` (Blazor-Komponente) — Löschung

- Die Datei `src/Schnittstellenzentrale/Components/Shared/ApplicationCard.razor` wird vollständig gelöscht.
- Recherche bestätigt: `<ApplicationCard` wird in keiner anderen Razor-Datei und in keiner C#-Datei referenziert. Die Komponente ist ein Dead Code.

## Datenbankmigrationen

Keine.

## Validierungsregeln

Keine. Die Sichtbarkeitsbedingung (`InterfaceType.OData` und `InterfaceUrl` nicht leer) ist bereits serverseitig beim Anlegen der Anwendung sichergestellt.

## Konfigurationsänderungen

Keine.

## Seiteneffekte und Risiken

- **Lokalisierungsdateien:** Durch das Entfernen von `ApplicationCard.razor` werden die sieben `ApplicationCard_*`-Schlüssel in beiden resx-Dateien zu totem Code. Sie werden im gleichen Schritt entfernt, um die resx-Dateien konsistent zu halten.
- **Hilfedokumentation:** Die Dateien `docs/help/anwendungen/beschreibung.md`, `docs/help/endpunkte/beschreibung.md`, `docs/help/endpunkte/ablauf-technisch.md`, `docs/help/schnittstellenzentrale/ablauf-technisch.md` und `docs/help/endpunkte/business-rules.md` referenzieren `ApplicationCard` in der Beschreibung des OData-Import-Ablaufs. Diese Dokumente liegen außerhalb des Quellcodes und sind nicht Bestandteil dieser Anforderung — eine Aktualisierung sollte als separate Aufgabe erfolgen.
- **Playwright-Tests (`ODataImportTests`):** Die Tests suchen den Button bereits per Rollenname in der Detailansicht. Nach der Implementierung werden sie grün; da `ApplicationCard` aus dem DOM verschwindet, entfällt das Kollisionsrisiko vollständig.

## Umsetzungsreihenfolge

1. `ApplicationCard.razor` löschen
2. Alle `ApplicationCard_*`-Schlüssel aus `SharedResources.resx` (EN) entfernen
3. Alle `ApplicationCard_*`-Schlüssel aus `SharedResources.de.resx` (DE) entfernen
4. Lokalisierungsschlüssel `ApplicationContentView_Button_ODataImport` in `SharedResources.resx` (EN) eintragen
5. Lokalisierungsschlüssel `ApplicationContentView_Button_ODataImport` in `SharedResources.de.resx` (DE) eintragen
6. `ApplicationContentView.razor` erweitern:
   a. `@inject IODataImportService ODataImportService` ergänzen
   b. Felder `_showODataImport`, `_odataDiff`, `_errorMessage` im `@code`-Block ergänzen
   c. Methoden `OpenODataImportAsync` und `CloseODataImport` implementieren
   d. Inline-Fehleranzeige (roter Alert) im Markup ergänzen
   e. Bedingten OData-Import-Button im `sz-hero-right`-Abschnitt ergänzen
   f. `ODataImportDialog`-Einbindung im Markup ergänzen

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `ODataImportButton_VisibleForODataApplication` | Neue Testklasse `ApplicationContentViewTests` (Unit/Bunit) | Button ist sichtbar, wenn `InterfaceType == OData` und `InterfaceUrl` nicht leer |
| `ODataImportButton_HiddenForRestApplication` | `ApplicationContentViewTests` | Button ist nicht sichtbar, wenn `InterfaceType != OData` |
| `ODataImportButton_HiddenWhenInterfaceUrlEmpty` | `ApplicationContentViewTests` | Button ist nicht sichtbar, wenn `InterfaceUrl` leer ist, obwohl `InterfaceType == OData` |
| `OpenODataImport_OnError_ShowsErrorMessage` | `ApplicationContentViewTests` | Wenn `ImportDiff.ErrorMessage` gesetzt ist, wird kein Dialog geöffnet und die inline-Fehlermeldung ist sichtbar |
| `OpenODataImport_OnSuccess_OpensDialog` | `ApplicationContentViewTests` | Bei erfolgreichem Import wird `ODataImportDialog` angezeigt |

### Betroffene bestehende Tests

Keine. `ApplicationCard` wird in keiner bestehenden Testdatei direkt referenziert.

### E2E-Tests (Pflicht)

| Szenario | Testdatei / Testklasse | Abgedecktes Akzeptanzkriterium |
|----------|------------------------|-------------------------------|
| OData-Import-Button sichtbar, Dialog öffnet sich, Endpunkte werden importiert | `ODataImportTests.ImportOData_RecognizesODataType_AndImportsEndpoints` (bereits vorhanden) | Happy Path: Button in Detailansicht klickbar, Dialog erscheint, Import-Ergebnis korrekt |
| Import persistiert nach Navigation | `ODataImportTests.ImportOData_CrudOperation_PersistsChange` (bereits vorhanden) | Importierte Endpunkte bleiben nach Seitenwechsel erhalten |

Welche bestehenden E2E-Tests müssen angepasst werden?

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| `ODataImportTests` | Tests suchen den Button bereits per Rollenname in der Detailansicht — nach Implementierung werden sie ohne Selektor-Anpassung grün. Das Kollisionsrisiko durch `ApplicationCard`-DOM-Reste entfällt durch die Löschung der Komponente. |

## Offene Punkte

Keine.
