# Schnittstellenzentrale

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)
![Blazor](https://img.shields.io/badge/Blazor-Server-512BD4?logo=blazor&logoColor=white)
![EF Core](https://img.shields.io/badge/EF_Core-9-512BD4?logo=dotnet&logoColor=white)
![xUnit](https://img.shields.io/badge/Tests-xUnit%20%2B%20Playwright-green?logo=xunit&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-Windows%20%2F%20IIS-0078D4?logo=windows&logoColor=white)

Blazor Server-Anwendung zur zentralen Verwaltung lokaler Webservice-Endpunkte. Sie ermöglicht das Anlegen, Gruppieren und direkte Ausführen von HTTP-Endpunkten mit verschiedenen Authentifizierungstypen, den Import aus Swagger/OpenAPI- und OData-Definitionen sowie Echtzeit-Synchronisation im Teammodus über SignalR.

## Features

- **Workspaces** – Anwendungen in Sammlungen organisieren, Endpunkte anlegen und direkt ausführen
- **Environments** – Systemumgebungen mit Variablentabellen verwalten
- **History** – Vollständige Aufrufhistorie mit Zeitraumfilter und Top-5-Auswertung je Anwendung
- **Import** – Endpunkte aus Swagger/OpenAPI- oder OData-`$metadata`-Definitionen importieren (Diff-Vorschau mit selektiver Übernahme); der jeweilige Import-Button erscheint in der Anwendungsdetailansicht, sobald eine passende Interface-URL hinterlegt ist
- **OData v4-API** – vollständiger OData v4-Service unter `/odatav4` mit CSDL-Metadaten-Dokument (`/odatav4/$metadata`); exponiert alle vier Kernobjekte als Entity-Sets mit CRUD-Zugriff, `$filter`, `$select`, `$expand`, `$orderby`, `$top` und `$skip`
  - **OData-Authentifizierung** – Tokens via `GET` oder `POST /odatav4/Authenticate()` (Windows/Negotiate)
  - **OData-Metadaten-Import** – Importiert Entity-Sets und Operationen aus fremden OData-Services; erzeugt je Entity-Set fünf Endpunkte (GET, POST, PUT/PATCH/DELETE mit `{key}`-Platzhalter); unterstützt proprietäre CSDL-Annotationen: `x-sz-bearer-token` (Token-Hinterlegung), `x-sz-auth-type` (Authentifizierungstyp), `x-sz-post-request-script` (Post-Request-Skript), `x-sz-header-{name}` (Custom-Header)
  - **API-Endpunkt für Import-Anwendung** – `POST /api/applications/{id}/odata-import/apply` wendet Import-Diffs auf Anwendungen an
  - **Storage-Mode-Header** – `X-Storage-Mode` wird von OData-Controllern ausgewertet (`Team`/`User`) zur optionalen Datenbereichs-Filterung
- **Authentifizierungstypen** – None, Basic, Negotiate, BearerToken, NegotiateWithImpersonation (Credentials im Windows Credential Manager)
- **Team-/Benutzermodus** – Umschaltung zwischen globalem und benutzerspezifischem Speicherbereich; die gewählte Einstellung wird im `localStorage` des Browsers gespeichert und beim nächsten Seitenaufruf automatisch wiederhergestellt
- **Echtzeit-Synchronisation** – SignalR-basiertes Broadcasting von Änderungen an alle verbundenen Sitzungen
- **Icons und Links** – Sammlungen und Anwendungen können mit Icon (PNG/JPEG) und URL-Links versehen werden
- **Health-Check** – Erreichbarkeit einer Anwendungs-URL prüfen (mit konfigurierbarem Cooldown)
- **Mehrsprachigkeit** – Benutzeroberfläche auf Deutsch und Englisch; Sprachauswahl automatisch über den `Accept-Language`-Header des Browsers (Englisch ist Standard und Fallback)
- **Impressum** – Optionale Impressumsseite unter `/impressum`; Inhalt wird aus einer Markdown-Datei (`impressum.md`) im Programmverzeichnis eingelesen und als HTML gerendert. Existiert die Datei nicht, bleibt der Navigationseintrag ausgeblendet — aktivierbar ohne Neustart durch einfaches Ablegen der Datei

## Voraussetzungen

- Windows Server oder Windows-Arbeitsstation mit IIS
- .NET 9.0 Runtime (ASP.NET Core)
- IIS: Windows-Authentifizierung aktiviert, Anonyme Authentifizierung deaktiviert
- SQLite (keine weitere Installation) oder SQL Server

## Installation

1. Anwendung publizieren:
   ```
   dotnet publish src/Schnittstellenzentrale/Schnittstellenzentrale.csproj -c Release -o publish/
   ```
2. IIS-Site anlegen; Anwendungspool auf `.NET CLR-Version: Kein verwalteter Code` setzen.
3. In IIS Windows-Authentifizierung aktivieren und Anonyme Authentifizierung deaktivieren.
4. `appsettings.json` im Veröffentlichungsverzeichnis anpassen (siehe Konfiguration).
5. Schreibzugriff des Anwendungspools auf das Veröffentlichungsverzeichnis sicherstellen (SQLite-Datei, `logs/`).
6. Anwendung starten — EF-Core-Migrationen werden automatisch beim Start angewendet (`Database.MigrateAsync`).

## Konfiguration

Alle Einstellungen in `appsettings.json`:

| Parameter | Typ | Standardwert | Beschreibung |
|-----------|-----|--------------|--------------|
| `DatabaseProvider` | `string` | `"SQLite"` | `SQLite` oder `SqlServer` |
| `ConnectionStrings:Default` | `string` | `"Data Source=schnittstellenzentrale.db"` | Verbindungszeichenfolge |
| `HealthCheck:CooldownSeconds` | `int` | `60` | Mindestabstand in Sekunden zwischen zwei Health-Checks derselben Anwendung |
| `Upload:MaxIconSizeBytes` | `int` | `524288` | Maximale Icon-Dateigröße in Bytes (512 KB) |
| `History:DefaultPageSize` | `int` | `50` | Einträge pro Seite in der Aufrufhistorie |
| `Serilog:MinimumLevel` | `string` | `"Information"` | Log-Level: `Verbose`, `Debug`, `Information`, `Warning`, `Error` |
| `Impressum:FilePath` | `string` | `""` (leer) | Pfad zur Impressum-Markdown-Datei; leer = `AppContext.BaseDirectory/impressum.md`. Relativer Pfad wird relativ zu `AppContext.BaseDirectory` aufgelöst, absoluter Pfad direkt verwendet. |

**Beispiel (SQLite):**

```json
{
  "DatabaseProvider": "SQLite",
  "ConnectionStrings": {
    "Default": "Data Source=schnittstellenzentrale.db"
  },
  "HealthCheck": { "CooldownSeconds": 60 },
  "Upload": { "MaxIconSizeBytes": 524288 },
  "History": { "DefaultPageSize": 50 },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "EventLog", "Args": { "source": "Schnittstellenzentrale", "logName": "Application" } },
      { "Name": "File", "Args": { "path": "logs/log-.txt", "rollingInterval": "Day", "retainedFileCountLimit": 7 } }
    ]
  },
  "Impressum": {
    "FilePath": ""
  }
}
```

**SQL Server:**

```json
{
  "DatabaseProvider": "SqlServer",
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=Schnittstellenzentrale;Integrated Security=True;TrustServerCertificate=True"
  }
}
```

## API

Die Schnittstellenzentrale stellt zwei API-Oberflächen bereit:

- **REST-API** unter `/api` — Token über `POST /authenticate`
- **OData v4-API** unter `/odatav4` — Token über `GET /odatav4/authenticate` oder `POST /odatav4/authenticate`

Die Authentifizierung erfolgt mittels Windows-Authentifizierung (Negotiate), die ein kurzlebiges Bearer-Token liefert. Dieses Token wird bei jedem Folgeaufruf im `Authorization`-Header übergeben und nach jeder erfolgreichen Anfrage rotiert (`X-New-Token`-Response-Header).

### REST-API (`/api`)

| Ressource | Beschreibung |
|-----------|--------------|
| `POST /authenticate` | Bearer-Token beziehen (Windows/Negotiate) |
| `/api/application-groups` | CRUD für `ApplicationGroup` |
| `/api/applications` | CRUD für `Application` |
| `POST /api/applications/{id}/import` | Import-Diff berechnen (Swagger oder OData, je nach `InterfaceType`) |
| `POST /api/applications/{id}/odata-import/apply` | OData-Import-Diff auf Anwendung anwenden |
| `/api/endpoint-groups` | CRUD für `EndpointGroup` |
| `/api/endpoints` | CRUD für `Endpoint` inkl. Header- und Query-Parameter-Routen |

Alle Endpunkte erfordern Bearer-Token im `Authorization`-Header. Unterstützt `X-Storage-Mode` (`Team`/`User`) und `X-Owner` für benutzermodus-abhängige Filterung. Schreibzugriffe lösen SignalR-Benachrichtigungen an alle verbundenen Clients aus.

### OData v4-API (`/odatav4`)

| Ressource | Beschreibung |
|-----------|--------------|
| `GET /odatav4/Authenticate()`<br/>`POST /odatav4/Authenticate()` | Bearer-Token beziehen (Windows/Negotiate) |
| `GET /odatav4/$metadata` | CSDL-Metadaten-Dokument (öffentlich zugänglich) |
| `GET/POST /odatav4/Applications`<br/>`GET/PUT/PATCH/DELETE /odatav4/Applications({id})` | CRUD für `Application` |
| `GET/POST /odatav4/ApplicationGroups`<br/>`GET/PUT/PATCH/DELETE /odatav4/ApplicationGroups({id})` | CRUD für `ApplicationGroup` |
| `GET/POST /odatav4/Endpoints`<br/>`GET/PUT/PATCH/DELETE /odatav4/Endpoints({id})` | CRUD für `Endpoint` |
| `GET/POST /odatav4/EndpointGroups`<br/>`GET/PUT/PATCH/DELETE /odatav4/EndpointGroups({id})` | CRUD für `EndpointGroup` |

**Authentifizierung & Token:** Alle Daten-Endpunkte erfordern Bearer-Token im `Authorization`-Header. Token können via `GET /odatav4/Authenticate()` oder `POST /odatav4/Authenticate()` (Windows/Negotiate) bezogen werden. Nach jeder erfolgreichen Anfrage wird ein neuer Token im `X-New-Token`-Response-Header bereitgestellt.

**Abfrageoptionen:** Auf Collection-Endpunkten werden `$filter`, `$select`, `$expand`, `$orderby`, `$top`, `$skip` und `$count` unterstützt.

**Storage-Mode-Filter:** Der `X-Storage-Mode`-Header (`Team` oder `User`, Standard: `User`) filtert die Datensätze nach Speichermodus-Zugehörigkeit. Wird nicht gesetzt, liefert die API standardmäßig Benutzer-Datensätze.

**Besonderheiten:** `Id` und `RowVersion` sind bei POST/PUT/PATCH nicht schreibbar. Systemeinträge (`IsSystem = true`) sind vor Änderungen und Löschungen geschützt (403). PATCH-Anfragen akzeptieren nur die zu ändernden Felder; `null`-Werte setzen Felder auf ihren leeren Zustand.

**OData-Metadaten-Import:** Das unter `/odatav4/$metadata` exponierte CSDL-Dokument wird auch intern für den OData-Import verwendet. Externe OData-Services können importiert werden. Der Import erzeugt je Entity-Set fünf Endpunkte (GET, POST, PUT/PATCH/DELETE mit `{key}`-Platzhalter) sowie einen Endpunkt je OData-Aktion und -Funktion. Unterstützte CSDL-Annotationen: `x-sz-bearer-token` (Token-Hinterlegung), `x-sz-auth-type` (Authentifizierungstyp), `x-sz-post-request-script` (Post-Request-Skript), `x-sz-header-{name}` (Custom-Header).

Ausführliche Dokumentation: [docs/help/api/odata-api.md](docs/help/api/odata-api.md)

## Projektstruktur

```
src/
├── Schnittstellenzentrale/          # Blazor Server App (Einstiegspunkt)
│   ├── Components/                  # Razor-Komponenten (Pages, Layout, Shared)
│   ├── wwwroot/                     # Statische Assets, CSS (sz-*-System)
│   └── Program.cs
├── Schnittstellenzentrale.Core/     # Domänenmodell, Interfaces, Services
├── Schnittstellenzentrale.Infrastructure/  # EF Core, Repositories, Migrationen
└── Schnittstellenzentrale.Tests/    # Unit-, Integrations- und Playwright-E2E-Tests
```

**Technologien:** ASP.NET Core 9 · Blazor Server · Entity Framework Core · ShadcnBlazor · SignalR · Microsoft.AspNetCore.OData 9.4.1 · Serilog · Markdig · xUnit · Playwright

## Tests

**Unit- und Integrationstests** (ohne Browser):

```
dotnet test --filter "FullyQualifiedName!~Playwright"
```

**Playwright-E2E-Tests** (Chromium wird beim ersten Build automatisch heruntergeladen):

```
dotnet test --filter "FullyQualifiedName~Playwright"
```

Beim ersten Build nach dem Klonen lädt das MSBuild-Target `InstallPlaywright` den Chromium-Browser herunter. Zum Überspringen in CI-Umgebungen:

```
dotnet test -p:SkipPlaywrightInstall=true
```

Trace-Dateien der Playwright-Tests werden nach jedem Lauf unter `playwright-traces/` abgelegt (auch bei Erfolg). Einsehen mit:

```
npx playwright show-trace playwright-traces/ApplicationCrudTests.zip
```

Abgedeckte E2E-Szenarien: Startseite, Anwendungs-CRUD, Endpunkt-Ausführung, Swagger-Import, OData-Import, Health-Check, Speichermoduswechsel, SignalR-Echtzeitsynchronisation.

## Lizenz

Dieses Projekt steht unter der [MIT-Lizenz](LICENSE). Hinweise zu Betreiberpflichten (Impressum, Datenschutz, gesetzliche Anforderungen) finden sich in [LEGAL.md](LEGAL.md).
