# Lokalisierung

## `SharedResources.resx` (EN Fallback)
Datei: `src/Schnittstellenzentrale/Resources/SharedResources.resx`

### Vorhandene Schlüssel mit OData-Bezug

| Schlüssel | Wert | Kommentar |
|-----------|------|-----------|
| `ODataImportDialog_Title` | `OData Import Preview` | Titel des OData-Import-Vorschau-Dialogs |
| `ApplicationCard_Button_ODataImport` | `OData Import` | Button-Beschriftung in `ApplicationCard` |

### Vorhandene Schlüssel für `ApplicationContentView` (Auswahl)

| Schlüssel | Wert |
|-----------|------|
| `ApplicationContentView_Button_SwaggerImport` | `Swagger Import` |
| `ApplicationContentView_Button_HealthCheck` | `Health Check` |
| `ApplicationContentView_Label_Description` | `Description` |
| `ApplicationContentView_Label_BaseUrl` | `Base URL` |
| `ApplicationContentView_Label_InterfaceUrl` | `Swagger / OData URL` |
| `ApplicationContentView_Label_Kpi` | `KPI` |
| `ApplicationContentView_Label_EndpointCount` | `Number of Endpoints` |
| `ApplicationContentView_NoDescription` | `No description available.` |

### Fehlender Schlüssel

| Schlüssel | Vorbild-Schlüssel |
|-----------|------------------|
| `ApplicationContentView_Button_ODataImport` | `ApplicationCard_Button_ODataImport` |

---

## `SharedResources.de.resx` (DE)
Datei: `src/Schnittstellenzentrale/Resources/SharedResources.de.resx`

### Vorhandene Schlüssel mit OData-Bezug

| Schlüssel | Wert |
|-----------|------|
| `ODataImportDialog_Title` | `OData-Import-Vorschau` |
| `ApplicationCard_Button_ODataImport` | `OData-Import` |

### Fehlender Schlüssel

| Schlüssel | Erwarteter Wert (analog zur ApplicationCard) |
|-----------|---------------------------------------------|
| `ApplicationContentView_Button_ODataImport` | `OData-Import` |
