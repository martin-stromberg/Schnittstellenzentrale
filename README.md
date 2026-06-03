# Schnittstellenzentrale

Zentrale Verwaltungsoberfläche für lokale Webservice-Endpunkte. Die Anwendung ermöglicht das Anlegen, Gruppieren und direkte Ausführen von HTTP-Endpunkten mit verschiedenen Authentifizierungstypen, den Import aus Swagger/OpenAPI- und OData-Definitionen sowie Echtzeit-Synchronisation im Teammodus über SignalR.

## Features

- **Workspaces** – Anwendungen in Sammlungen organisieren, Endpunkte anlegen und direkt ausführen
- **Environments** – Systemumgebungen mit Variablentabellen verwalten
- **History** – Vollständige Aufrufhistorie mit Zeitraumfilter und Top-5-Auswertung je Anwendung
- **Import** – Endpunkte aus Swagger/OpenAPI- oder OData-`$metadata`-Definitionen importieren (Diff-Vorschau mit selektiver Übernahme)
- **Authentifizierungstypen** – None, Basic, Negotiate, BearerToken, NegotiateWithImpersonation (Credentials im Windows Credential Manager)
- **Team-/Benutzermodus** – Umschaltung zwischen globalem und benutzerspezifischem Speicherbereich
- **Echtzeit-Synchronisation** – SignalR-basiertes Broadcasting von Änderungen an alle verbundenen Sitzungen
- **Icons und Links** – Sammlungen und Anwendungen können mit Icon (PNG/JPEG) und URL-Links versehen werden
- **Health-Check** – Erreichbarkeit einer Anwendungs-URL prüfen (mit konfigurierbarem Cooldown)

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

**Technologien:** ASP.NET Core 9 · Blazor Server · Entity Framework Core · ShadcnBlazor · SignalR · Serilog · xUnit · Playwright

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

Abgedeckte E2E-Szenarien: Startseite, Anwendungs-CRUD, Endpunkt-Ausführung, Swagger-Import, Health-Check, Speichermoduswechsel, SignalR-Echtzeitsynchronisation.
