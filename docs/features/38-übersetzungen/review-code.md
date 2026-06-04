# Code-Review (2. Durchlauf): Feature 38 – Mehrsprachigkeit DE/EN

**Branch:** `38-übersetzungen`  
**Verglichen mit:** `main`  
**Scope:** Alle gestagten und ungestagten Änderungen (Implementierung liegt uncommitted im Working Tree)

---

## Status

`Befunde vorhanden`

---

## Behobene Befunde aus dem 1. Durchlauf

| # | Befund | Status |
|---|--------|--------|
| 1 | Hardcodiertes `"wirklich löschen?"` in drei Confirm-Dialogen | ✅ Behoben — Suffix-Schlüssel `_MessageSuffix` eingeführt |
| 2 | `WorkspacesLayout.razor` komplett übersprungen | ✅ Behoben — vollständig lokalisiert |
| 3 | Fehlermeldungen in `ApplicationGroupTree.razor` und `ApplicationCard.razor` | ✅ Behoben |
| 4 | `EnvironmentContentView.razor` Lade-Text | ✅ Behoben |
| 5 | `ContentHeader.razor` Tooltip | ✅ Behoben |
| 6 | `EnvironmentsSidebar.razor` `_errorMessage`-Zuweisungen | ✅ Behoben |
| 7 | `CoreResources.resx`/`de.resx` tote Dateien | ✅ Nicht anwendbar (Designentscheid: alles in `SharedResources`) |
| 8 | Redundantes `AddLocalization()` | ✅ Behoben — nicht mehr vorhanden |

---

## Befunde

### 1. `ContentHeader.razor` — mehrere nutzerseitige Strings weiterhin hartcodiert

**Schweregrad: Hoch** — Der Tooltip wurde lokalisiert, die übrigen sichtbaren Texte nicht

Folgende Strings sind noch nicht über `IStringLocalizer` abgerufen:

| Zeile | Hartcodierter Text |
|-------|--------------------|
| 49 | `"Untertitel hinzufügen"` (Placeholder-Text im Untertitel-Feld) |
| 136 | `"Name darf nicht leer sein."` (Validierungsfehler, nutzerseitig sichtbar) |
| 202 | `"Nur PNG- und JPEG-Dateien sind erlaubt."` (Upload-Fehlermeldung) |
| 207 | `$"Datei ist zu groß (max. {maxSize / 1024} KB)."` (Upload-Fehlermeldung) |

Die Komponente hat `@inject IStringLocalizer<SharedResources> L` bereits (für den Tooltip), die weiteren Stellen wurden aber nicht nachgezogen.

Betroffene Datei: `src/Schnittstellenzentrale/Components/Shared/ContentHeader.razor`

---

### 2. `EnvironmentContentView.razor` — Beschreibungs-Bereich und Inline-Validierungen nicht lokalisiert

**Schweregrad: Hoch**

Folgende hartcodierten Strings sind nutzerseitig sichtbar:

| Zeile | Hartcodierter Text |
|-------|--------------------|
| 34 | `"Abbrechen"` (Button-Label im Beschreibungs-Bearbeitungsmodus) |
| 39 | `"Beschreibung hinzufügen…"` (Placeholder-Text) |
| 49 | `"Variablen"` (Abschnitts-Label) |
| 98 | `"Name darf nicht leer sein."` (Inline-Validierungsfehler) |
| 103 | `"Name darf maximal 200 Zeichen lang sein."` (Inline-Validierungsfehler) |

Die Komponente hat `@inject IStringLocalizer<SharedResources> L` bereits (für den Ladezustand), die weiteren Stellen wurden aber nicht nachgezogen.

Betroffene Datei: `src/Schnittstellenzentrale/Components/Shared/EnvironmentContentView.razor`

---

### 3. `EnvironmentsSidebar.razor` — Inline-Validierungsfehler weiterhin hartcodiert

**Schweregrad: Mittel**

Die UI-Texte (Buttons, Tooltips, Placeholder, Fehlermeldungen) wurden lokalisiert. Zwei Inline-Validierungsfehler in `ConfirmCreateAsync` wurden ausgelassen:

```csharp
// Zeile 96:
_createError = "Name ist ein Pflichtfeld.";
// Zeile 101:
_createError = "Name darf maximal 200 Zeichen lang sein.";
```

`_createError` wird dem Nutzer direkt in der Sidebar angezeigt (`<span class="sz-inplace-error">@_createError</span>`).

Betroffene Datei: `src/Schnittstellenzentrale/Components/Shared/EnvironmentsSidebar.razor`

---

### 4. `ApplicationCard.razor` — UI-Texte weitgehend nicht lokalisiert

**Schweregrad: Mittel**

Laut Befund 3 des 1. Durchlaufs wurde nur die `_errorMessage`-Zuweisung lokalisiert. Die sichtbaren UI-Labels und Button-Texte wurden nicht nachgezogen:

| Zeile | Hartcodierter Text |
|-------|--------------------|
| 20 | `"Swagger-Import"` (Button-Label) |
| 24 | `"OData-Import"` (Button-Label) |
| 26 | `"Health-Check"` (Button-Label) |
| 31 | `"Basis-URL:"` (Label) |
| 36 | `"Metadaten-URL:"` / `"Swagger-URL:"` (konditionaler Label-Text) |

Die Komponente hat `@inject IStringLocalizer<SharedResources> L` bereits (für die Fehlermeldung), aber kein Schlüssel für die Button-Labels und URL-Labels existiert in `SharedResources.resx`.

Betroffene Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationCard.razor`

---

### 5. `ApplicationContentView.razor`, `CollectionContentView.razor`, `FolderContentView.razor` — vollständig unlokalisiert

**Schweregrad: Mittel** — Keine dieser Komponenten wurde im 1. Durchlauf oder in der Implementierung erfasst

Alle drei Komponenten enthalten nutzerseitig sichtbare hartcodierte deutsche Texte und haben **kein** `@inject IStringLocalizer<SharedResources> L`:

**`ApplicationContentView.razor`** (kein Localizer injiziert):
- Zeile 30: `"Swagger-Import"` (Button)
- Zeile 32: `"Health-Check"` (Button)
- Zeile 50: `"Beschreibung"` (Abschnitts-Label)
- Zeile 52: `"Keine Beschreibung vorhanden."` (Leertext)
- Zeile 56: `"Basis-URL"` (URL-Label)
- Zeile 62: `"Swagger / OData URL"` (URL-Label)
- Zeile 72: `"KPI"` (Abschnitts-Label)
- Zeile 75: `"Anzahl Endpunkte"` (KPI-Label)

**`CollectionContentView.razor`** (kein Localizer injiziert):
- Zeile 21: `"+ Neue Anwendung"` (Button)
- Zeile 29: `"KPI"` (Abschnitts-Label)
- Zeile 32: `"Anzahl Anwendungen"` (KPI-Label)
- Zeile 37: `"Anzahl Endpunkte"` (KPI-Label)
- Zeile 52-55: Tabellen-Header `"Methode"`, `"Endpunkt"`, `"Beschreibung"`, `"Aktion"`
- Zeile 68: `title="Ausführen"` (Button-Tooltip)
- Zeile 75: `"Keine Endpunkte vorhanden."` (Leertext)

**`FolderContentView.razor`** (kein Localizer injiziert):
- Zeile 7: `"Endpunkte in »@EndpointGroup.Name«"` (Abschnittsüberschrift)
- Zeile 11-14: Tabellen-Header `"Methode"`, `"Endpunkt"`, `"Beschreibung"`, `"Aktion"`
- Zeile 29: `title="Ausführen"` (Button-Tooltip)

Betroffene Dateien:
- `src/Schnittstellenzentrale/Components/Shared/ApplicationContentView.razor`
- `src/Schnittstellenzentrale/Components/Shared/CollectionContentView.razor`
- `src/Schnittstellenzentrale/Components/Shared/FolderContentView.razor`

---

### 6. `HealthCheckDialog.razor` — Titel-Präfix hartcodiert

**Schweregrad: Niedrig**

Das Dialog-Title-Attribut lautet:
```razor
<SzDialog Title="@($"Health-Check: {Application.Name}")" ...>
```

Der Präfix `"Health-Check: "` ist hartcodiert. Bei englischsprachigen Nutzern wäre der Titel konsistent (da es ein englischer Begriff ist), aber die Konvention des Features erfordert auch technische Begriffe als Ressourcen-Schlüssel zu hinterlegen (vgl. `TopBar_TabWorkspaces` = "Workspaces" in der resx).

Betroffene Datei: `src/Schnittstellenzentrale/Components/Shared/HealthCheckDialog.razor`, Zeile 3

---

### 7. `ODataImportDialog.razor` und `SwaggerImportDialog.razor` — Titel-Strings hartcodiert

**Schweregrad: Niedrig**

Beide Dialoge übergeben ihren Titel als hartcodierten String an `ImportDialog`:
```razor
<ImportDialog Title="OData-Import-Vorschau" ... />
<ImportDialog Title="Swagger-Import-Vorschau" ... />
```

`ImportDialog` gibt diesen Titel direkt an `<SzDialog Title="@Title">` weiter. Da `ODataImportDialog` und `SwaggerImportDialog` laut plan.md bewusst keine eigene Lokalisierung erhalten sollen, müsste entweder `ImportDialog` den Titel lokalisieren (mit einem per Parameter gewählten Schlüssel) oder die aufrufenden Dialoge müssen einen Ressourcen-Schlüssel übergeben.

Betroffene Dateien:
- `src/Schnittstellenzentrale/Components/Shared/ODataImportDialog.razor`, Zeile 3
- `src/Schnittstellenzentrale/Components/Shared/SwaggerImportDialog.razor`, Zeile 3

---

### 8. `ApplicationGroupTree.razor` — `"Ohne Sammlung"` weiterhin hartcodiert

**Schweregrad: Niedrig**

An zwei Stellen ist der Begriff `"Ohne Sammlung"` hartcodiert, obwohl die Komponente `@inject IStringLocalizer<SharedResources> L` bereits hat und alle anderen Texte (Fehlermeldungen) lokalisiert wurden:

- Zeile 47: `<CollapsibleSection Title="Ohne Sammlung" ...>`  
- Zeile 426: `var targetGroup = ... ?? "Ohne Sammlung";` (für das Activity-Log)

In `SharedResources.resx` existiert kein entsprechender Schlüssel für diesen Begriff.

Betroffene Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationGroupTree.razor`, Zeilen 47 und 426

---

### 9. `WorkspacesLayout.razor` — Default-Endpunktname `"Neuer Endpunkt"` hartcodiert

**Schweregrad: Niedrig** — Sichtbar für den Nutzer als initialer Endpunkt-Name

In `HandleCreateEndpointRequested` (Zeile 287) wird ein neuer Endpunkt mit einem hartcodierten deutschen Namen angelegt:
```csharp
var endpoint = new Core.Models.Endpoint
{
    Name = "Neuer Endpunkt",
    ...
};
```

Dieser Name wird sofort in der Seitenleiste und auf der Endpunkt-Seite angezeigt und ist für den Nutzer sichtbar, bevor er den Namen ändert.

Betroffene Datei: `src/Schnittstellenzentrale/Components/Layout/WorkspacesLayout.razor`, Zeile 287

---

### 10. Localization-Integrationstests prüfen auf Text in `blazor-error-ui` — fragile Assertion

**Schweregrad: Niedrig** — Mögliche Falsch-Negative

`LocalizationTests` assertiert, dass ein GET auf `/` den Text `"Reload"` (EN) bzw. `"Neu laden"` (DE) enthält. Dieser Text stammt aus dem `<div id="blazor-error-ui">` in `AppShell.razor`, der via `@L["AppShell_ReloadLink"]` gerendert wird.

Das `blazor-error-ui`-Div ist im normalen Betrieb per CSS (`display: none`) ausgeblendet und wird nur bei einem unbehandelten Blazor-Fehler eingeblendet. Ob der Text im initialen SSR-HTML-Response enthalten ist, hängt davon ab, ob Blazor Server das gesamte Komponenten-HTML statisch vorrendert. Bei Blazor Server mit interaktivem Server-Rendering wird die Seite initial als statisches HTML geliefert — das `AppShell`-Markup (inklusive `blazor-error-ui`) ist im HTML enthalten, also sollte die Assertion funktionieren.

Allerdings ist der Text `"Reload"` ein sehr generischer String, der in anderem Seiteninhalt auftreten könnte (False Positive) oder bei einer Renderingänderung fehlen könnte (False Negative). Eine robustere Assertion wäre ein spezifischerer String oder ein `data-testid`-Attribut.

Betroffene Datei: `src/Schnittstellenzentrale.Tests/Integration/LocalizationTests.cs`
